using System.Net.Http.Json;
using System.Text.Json;
using JurisApp.Application.Interfaces.AI;
using JurisApp.Domain.Entities;
using JurisApp.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace JurisApp.Infrastructure.AI;

public class AIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;

    public AIService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _model = configuration["AI:Claude:Model"]
            ?? configuration["AI:Model"]
            ?? throw new InvalidOperationException("AI:Claude:Model is not configured.");
    }

    public async Task<string> SendChatMessageAsync(
        string userMessage,
        IReadOnlyList<Message> previousMessages,
        IReadOnlyList<CustomSkill> activeSkills,
        CancellationToken cancellationToken = default)
    {
        var systemPrompt = BuildChatSystemPrompt(activeSkills);
        var messages = BuildMessageHistory(previousMessages, userMessage);
        return await SendRequestAsync(systemPrompt, messages, cancellationToken);
    }

    public async Task<DocumentAnalysisResult> AnalyzeDocumentAsync(
        string documentText,
        DocumentAnalysisType analysisType,
        IReadOnlyList<CustomSkill> activeSkills,
        CancellationToken cancellationToken = default)
    {
        var systemPrompt = BuildAnalysisSystemPrompt(analysisType, activeSkills);
        var messages = new[]
        {
            new { role = "user", content = $"Analyze the following document:\n\n{documentText}" }
        };

        var raw = await SendRequestAsync(systemPrompt, messages, cancellationToken);
        return ParseAnalysisResult(raw);
    }

    public async Task<string> CreateTaskPlanAsync(
        string description,
        CancellationToken cancellationToken = default)
    {
        var systemPrompt =
            "You are a legal task planner. Given a task description, " +
            "produce a clear, step-by-step action plan in plain text.";

        var messages = new[]
        {
            new { role = "user", content = $"Create an action plan for: {description}" }
        };

        return await SendRequestAsync(systemPrompt, messages, cancellationToken);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    // NOTE: This method builds the request body using the Anthropic messages API
    // shape. If the provider changes, only this method and the HttpClient
    // base address / headers in DependencyInjection need to be updated.
    private async Task<string> SendRequestAsync(
        string systemPrompt,
        object messages,
        CancellationToken cancellationToken)
    {
        var body = new
        {
            model = _model,
            max_tokens = 2048,
            system = systemPrompt,
            messages
        };

        var response = await _httpClient.PostAsJsonAsync("messages", body, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);

        // Anthropic response shape: { content: [ { type: "text", text: "..." } ] }
        return doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? string.Empty;
    }

    private static string BuildChatSystemPrompt(IReadOnlyList<CustomSkill> activeSkills)
    {
        var basePrompt =
            "You are JurisApp, an AI legal assistant. " +
            "Provide accurate, professional legal guidance. " +
            "Always clarify that your responses are informational and not formal legal advice.";

        if (!activeSkills.Any())
            return basePrompt;

        var skillInstructions = activeSkills
            .Where(s => s.IsActive)
            .Select(s =>
                $"## Skill: {s.Name}\n" +
                $"When to use: {s.WhenToUse}\n" +
                $"Instructions: {s.Instructions}\n" +
                $"Examples: {s.Examples}\n" +
                $"Red flags: {s.RedFlags}\n" +
                $"Output format: {s.OutputFormat}");

        return basePrompt + "\n\n" + string.Join("\n\n", skillInstructions);
    }

    private static string BuildAnalysisSystemPrompt(
        DocumentAnalysisType type,
        IReadOnlyList<CustomSkill> activeSkills)
    {
        var typeInstruction = type switch
        {
            DocumentAnalysisType.Summary =>
                "Provide a structured summary of the document.",
            DocumentAnalysisType.RiskAnalysis =>
                "Identify and explain all legal risks present in the document.",
            DocumentAnalysisType.ContractReview =>
                "Review the contract and highlight key clauses, obligations, and concerns.",
            DocumentAnalysisType.Custom =>
                "Analyze the document thoroughly according to any active custom skills.",
            _ => "Analyze the document."
        };

        var basePrompt =
            "You are JurisApp, an AI legal document analyst. " +
            $"{typeInstruction} " +
            "Respond ONLY with a JSON object with these exact keys: " +
            "\"summary\", \"risks\", \"recommendations\", \"references\". " +
            "Do not include any text outside the JSON object.";

        if (!activeSkills.Any())
            return basePrompt;

        var skillInstructions = activeSkills
            .Where(s => s.IsActive)
            .Select(s => $"- {s.Name}: {s.Instructions}");

        return basePrompt + "\n\nApply these custom skills:\n" +
               string.Join("\n", skillInstructions);
    }

    private static object[] BuildMessageHistory(
        IReadOnlyList<Message> previousMessages,
        string userMessage)
    {
        var history = previousMessages
            .Where(m => m.Role != MessageRole.System)
            .Select(m => new
            {
                role = m.Role == MessageRole.User ? "user" : "assistant",
                content = m.Content
            })
            .Cast<object>()
            .ToList();

        history.Add(new { role = "user", content = userMessage });
        return history.ToArray();
    }

    private static DocumentAnalysisResult ParseAnalysisResult(string raw)
    {
        try
        {
            using var doc = JsonDocument.Parse(raw.Trim());
            var root = doc.RootElement;

            return new DocumentAnalysisResult
            {
                Summary         = root.GetProperty("summary").GetString()         ?? string.Empty,
                Risks           = root.GetProperty("risks").GetString()            ?? string.Empty,
                Recommendations = root.GetProperty("recommendations").GetString()  ?? string.Empty,
                References      = root.GetProperty("references").GetString()       ?? string.Empty
            };
        }
        catch
        {
            return new DocumentAnalysisResult
            {
                Summary         = raw,
                Risks           = string.Empty,
                Recommendations = string.Empty,
                References      = string.Empty
            };
        }
    }
}
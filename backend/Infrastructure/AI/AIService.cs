using System.Net.Http.Json;
using System.Text.Json;
using JurisApp.Application.Common;
using JurisApp.Application.Interfaces.AI;
using JurisApp.Domain.Entities;
using JurisApp.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JurisApp.Infrastructure.AI;

public class AIService : IAIService
{
    private const string DevFallbackReply = "Respuesta simulada de IA en modo desarrollo.";

    private readonly HttpClient _httpClient;
    private readonly ClaudeOptions _options;
    private readonly ILogger<AIService> _logger;

    public AIService(
        HttpClient httpClient,
        IOptions<ClaudeOptions> options,
        ILogger<AIService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> SendChatMessageAsync(
        string userMessage,
        IReadOnlyList<Message> previousMessages,
        IReadOnlyList<CustomSkill> activeSkills,
        CancellationToken cancellationToken = default)
    {
        if (!IsLiveMode())
            return DevFallbackReply;

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
        if (!IsLiveMode())
        {
            return new DocumentAnalysisResult
            {
                Summary = DevFallbackReply,
                Risks = string.Empty,
                Recommendations = string.Empty,
                References = string.Empty
            };
        }

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
        if (!IsLiveMode())
            return DevFallbackReply;

        var systemPrompt =
            "You are a legal task planner. Given a task description, " +
            "produce a clear, step-by-step action plan in plain text.";

        var messages = new[]
        {
            new { role = "user", content = $"Create an action plan for: {description}" }
        };

        return await SendRequestAsync(systemPrompt, messages, cancellationToken);
    }

    private bool IsLiveMode() =>
        _options.Enabled && !string.IsNullOrWhiteSpace(_options.ApiKey);

    private async Task<string> SendRequestAsync(
        string systemPrompt,
        object messages,
        CancellationToken cancellationToken)
    {
        var requestUrl = $"{_options.BaseUrl.TrimEnd('/')}/v1/messages";

        var body = new
        {
            model = _options.Model,
            max_tokens = _options.MaxTokens,
            system = systemPrompt,
            messages
        };

        _logger.LogInformation(
            "Claude request → URL: {Url}, Model: {Model}",
            requestUrl,
            _options.Model);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync("/v1/messages", body, cancellationToken);
        }
        catch (Exception ex) when (ex is not AIServiceException)
        {
            _logger.LogError(ex, "Error de red al llamar a Claude en {Url}", requestUrl);
            throw new AIServiceException("No se pudo conectar con el servicio de IA.", ex);
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Claude error → Status: {StatusCode}, URL: {Url}, Model: {Model}, Body: {Body}",
                (int)response.StatusCode,
                requestUrl,
                _options.Model,
                responseBody);

            throw new AIServiceException(
                $"El servicio de IA respondió con error {(int)response.StatusCode}.");
        }

        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Respuesta inesperada de Claude: {Body}", responseBody);
            throw new AIServiceException("La respuesta del servicio de IA no tiene el formato esperado.", ex);
        }
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

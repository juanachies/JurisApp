using System.Text.Json;
using JurisApp.Application.DTOs.AITasks;
using JurisApp.Application.Interfaces.AI;
using JurisApp.Domain.Entities;
using JurisApp.Domain.Enums;

namespace JurisApp.Infrastructure.AI;

public class AIService : IAIService
{
    private const string DevFallbackReply = "Respuesta simulada de IA en modo desarrollo.";

    private readonly AnthropicMessageClient _client;

    public AIService(
        AnthropicMessageClient client)
    {
        _client = client;
    }

    public async Task<string> SendChatMessageAsync(
        string userMessage,
        IReadOnlyList<Message> previousMessages,
        IReadOnlyList<CustomSkill> activeSkills,
        IReadOnlyList<ChatDocumentContext>? chatDocuments = null,
        CancellationToken cancellationToken = default)
    {
        if (!_client.IsLiveMode())
            return DevFallbackReply;

        var systemPrompt = BuildChatSystemPrompt(activeSkills, chatDocuments);
        var messages = BuildMessageHistory(previousMessages, userMessage);
        return await _client.SendAsync(systemPrompt, messages, cancellationToken: cancellationToken);
    }

    public async Task<DocumentAnalysisResult> AnalyzeDocumentAsync(
        string documentText,
        DocumentAnalysisType analysisType,
        IReadOnlyList<CustomSkill> activeSkills,
        CancellationToken cancellationToken = default)
    {
        if (!_client.IsLiveMode())
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

        var raw = await _client.SendAsync(systemPrompt, messages, cancellationToken: cancellationToken);
        return ParseAnalysisResult(raw);
    }

    public async Task<StructuredTaskPlan> CreateStructuredTaskPlanAsync(
        string description,
        IReadOnlyList<Message> previousMessages,
        IReadOnlyList<CustomSkill> activeSkills,
        IReadOnlyList<ChatDocumentContext>? chatDocuments = null,
        CancellationToken cancellationToken = default)
    {
        if (!_client.IsLiveMode())
            return TaskPlanParser.BuildMockPlan(description);

        var systemPrompt =
            "Sos JurisApp, asistente legal para abogados en Argentina. " +
            "Dado un encargo del usuario, generá un plan de trabajo legal estructurado. " +
            "Respondé ÚNICAMENTE con un JSON válido con esta forma exacta: " +
            "{\"objective\":\"...\",\"summary\":\"...\",\"steps\":[{\"order\":1,\"title\":\"...\",\"description\":\"...\"}]}. " +
            "Incluí entre 5 y 8 pasos concretos adaptados al caso (hechos relevantes, documentación, riesgos, teoría del caso, esquema de demanda, intimación, próximos pasos). " +
            "Todo en español. Sin texto fuera del JSON.";

        var contextBlock = BuildTaskContextBlock(chatDocuments, activeSkills);
        var userContent =
            $"{contextBlock}\n\nEncargo del abogado:\n{description}";

        var messages = new[] { new { role = "user", content = userContent } };
        var raw = await _client.SendAsync(systemPrompt, messages, cancellationToken: cancellationToken);
        return TaskPlanParser.Parse(raw, description);
    }

    public async Task<string> ExecuteTaskStepAsync(
        string taskDescription,
        TaskStepDto step,
        IReadOnlyList<TaskStepDto> completedSteps,
        IReadOnlyList<Message> previousMessages,
        IReadOnlyList<CustomSkill> activeSkills,
        IReadOnlyList<ChatDocumentContext>? chatDocuments = null,
        CancellationToken cancellationToken = default)
    {
        if (!_client.IsLiveMode())
        {
            return $"[Paso {step.Order} simulado: {step.Title}]\n\n" +
                   $"Resultado de desarrollo para: {step.Description}\n\n" +
                   "Configurá AI:UseMock=false y una API key real para ejecutar con Claude.";
        }

        var systemPrompt =
            "Sos JurisApp, asistente legal para abogados en Argentina. " +
            "Estás ejecutando UN paso de un plan de trabajo legal. " +
            "Entregá un resultado profesional, concreto y accionable en español. " +
            "No repitas el encargo completo; enfocate solo en este paso.";

        var completedSummary = completedSteps.Count == 0
            ? "Ninguno."
            : string.Join("\n", completedSteps.Select(s => $"- Paso {s.Order} ({s.Title}): completado"));

        var contextBlock = BuildTaskContextBlock(chatDocuments, activeSkills);
        var userContent =
            $"{contextBlock}\n\nEncargo general:\n{taskDescription}\n\n" +
            $"Pasos ya completados:\n{completedSummary}\n\n" +
            $"Paso actual ({step.Order}): {step.Title}\n" +
            $"Instrucción del paso: {step.Description}";

        var messages = new[] { new { role = "user", content = userContent } };
        return await _client.SendAsync(systemPrompt, messages, cancellationToken: cancellationToken);
    }

    private static string BuildTaskContextBlock(
        IReadOnlyList<ChatDocumentContext>? chatDocuments,
        IReadOnlyList<CustomSkill> activeSkills)
    {
        var parts = new List<string>();

        if (chatDocuments is { Count: > 0 })
        {
            parts.Add("Documentos del chat:\n" + string.Join("\n\n",
                chatDocuments.Select(d => $"### {d.Title}\n{d.Content}")));
        }

        var skills = activeSkills.Where(s => s.IsActive).ToList();
        if (skills.Count > 0)
        {
            parts.Add("Skills activas:\n" + string.Join("\n",
                skills.Select(s => $"- {s.Name}: {s.Instructions}")));
        }

        return parts.Count == 0 ? string.Empty : string.Join("\n\n", parts);
    }

    private static string BuildChatSystemPrompt(
        IReadOnlyList<CustomSkill> activeSkills,
        IReadOnlyList<ChatDocumentContext>? chatDocuments = null)
    {
        var basePrompt =
            "You are JurisApp, an AI legal assistant. " +
            "Provide accurate, professional legal guidance. " +
            "Always clarify that your responses are informational and not formal legal advice.";

        if (chatDocuments is { Count: > 0 })
        {
            var documentBlocks = chatDocuments.Select(d =>
                $"### {d.Title}\n{d.Content}");

            basePrompt +=
                "\n\nThe following documents are attached to this chat. " +
                "You MUST take their content into account when answering:\n\n" +
                string.Join("\n\n", documentBlocks);
        }

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

        return basePrompt +
               "\n\nThe following custom skills are ACTIVE for this chat. " +
               "You MUST follow their instructions in your response:\n\n" +
               string.Join("\n\n", skillInstructions);
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
            DocumentAnalysisType.Segmented =>
                "Analyze the document thoroughly for segmented legal review.",
            _ => "Analyze the document."
        };

        var basePrompt =
            "You are JurisApp, an AI legal document analyst. " +
            $"{typeInstruction} " +
            "Respond ONLY with a JSON object with these exact keys: " +
            "\"summary\", \"risks\", \"recommendations\", \"references\". " +
            "Each value must be a plain string (use bullet points with newlines if needed). " +
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
        var json = JsonResponseHelper.StripMarkdownJson(raw);

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            return new DocumentAnalysisResult
            {
                Summary         = JsonResponseHelper.ReadJsonField(root, "summary"),
                Risks           = JsonResponseHelper.ReadJsonField(root, "risks"),
                Recommendations = JsonResponseHelper.ReadJsonField(root, "recommendations"),
                References      = JsonResponseHelper.ReadJsonField(root, "references")
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

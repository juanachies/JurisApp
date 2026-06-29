using System.Net.Http.Json;
using System.Text.Json;
using JurisApp.Application.Common;
using JurisApp.Application.DTOs.AITasks;
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
        IReadOnlyList<ChatDocumentContext>? chatDocuments = null,
        CancellationToken cancellationToken = default)
    {
        if (!IsLiveMode())
            return DevFallbackReply;

        var systemPrompt = BuildChatSystemPrompt(activeSkills, chatDocuments);
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

    public async Task<StructuredTaskPlan> CreateStructuredTaskPlanAsync(
        string description,
        IReadOnlyList<Message> previousMessages,
        IReadOnlyList<CustomSkill> activeSkills,
        IReadOnlyList<ChatDocumentContext>? chatDocuments = null,
        CancellationToken cancellationToken = default)
    {
        if (!IsLiveMode())
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
        var raw = await SendRequestAsync(systemPrompt, messages, cancellationToken);
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
        if (!IsLiveMode())
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
        return await SendRequestAsync(systemPrompt, messages, cancellationToken);
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

        var skills = activeSkills.ToList();
        if (skills.Count > 0)
        {
            parts.Add("Skills aplicadas:\n" + string.Join("\n",
                skills.Select(s => $"- {s.Name}: {s.Instructions}")));
        }

        return parts.Count == 0 ? string.Empty : string.Join("\n\n", parts);
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
            .Select(s =>
                $"## Skill: {s.Name}\n" +
                $"When to use: {s.WhenToUse}\n" +
                $"Instructions: {s.Instructions}\n" +
                $"Examples: {s.Examples}\n" +
                $"Red flags: {s.RedFlags}\n" +
                $"Output format: {s.OutputFormat}");

        return basePrompt +
               "\n\nThe following custom skills are applied to this chat. " +
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
        var json = StripMarkdownJson(raw);

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            return new DocumentAnalysisResult
            {
                Summary         = ReadJsonField(root, "summary"),
                Risks           = ReadJsonField(root, "risks"),
                Recommendations = ReadJsonField(root, "recommendations"),
                References      = ReadJsonField(root, "references")
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

    private static string StripMarkdownJson(string raw)
    {
        var trimmed = raw.Trim();

        if (!trimmed.StartsWith("```", StringComparison.Ordinal))
            return trimmed;

        var firstNewline = trimmed.IndexOf('\n');
        if (firstNewline < 0)
            return trimmed;

        trimmed = trimmed[(firstNewline + 1)..];

        if (trimmed.EndsWith("```", StringComparison.Ordinal))
            trimmed = trimmed[..^3];

        return trimmed.Trim();
    }

    private static string ReadJsonField(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var element))
            return string.Empty;

        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Array => string.Join(
                "\n",
                element.EnumerateArray().Select(item => item.ValueKind switch
                {
                    JsonValueKind.String => "- " + item.GetString(),
                    _ => "- " + item.ToString()
                })),
            JsonValueKind.Null => string.Empty,
            _ => element.ToString()
        };
    }
}

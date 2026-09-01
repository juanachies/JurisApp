using System.Text.Json;
using JurisApp.Application.DTOs.AITasks;
using JurisApp.Application.Interfaces.AI;
using JurisApp.Domain.Entities;
using JurisApp.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace JurisApp.Infrastructure.AI;

public class AIService : IAIService
{
    private const string DevFallbackReply = "Respuesta simulada de IA en modo desarrollo.";

    private readonly OpenAIMessageClient _client;
    private readonly IConfiguration _configuration;

    public AIService(OpenAIMessageClient client, IConfiguration configuration)
    {
        _client = client;
        _configuration = configuration;
    }

    public async Task<string> SendChatMessageAsync(
        string userMessage,
        IReadOnlyList<Message> previousMessages,
        IReadOnlyList<CustomSkill> activeSkills,
        IReadOnlyList<ChatDocumentContext>? chatDocuments = null,
        string? userProvince = null,
        CancellationToken cancellationToken = default)
    {
        if (!_client.IsLiveMode())
        {
            if (chatDocuments is { Count: > 0 })
            {
                return DevFallbackReply +
                       " Documentos en contexto: " +
                       string.Join("; ", chatDocuments.Select(d => d.Title));
            }

            return DevFallbackReply;
        }

        var systemPrompt = BuildChatSystemPrompt(activeSkills, chatDocuments, userProvince);
        var messages = BuildMessageHistory(previousMessages, userMessage);
        return await _client.SendAsync(systemPrompt, messages, cancellationToken: cancellationToken);
    }

    public async Task<DocumentAnalysisResult> AnalyzeDocumentAsync(
        string documentText,
        DocumentAnalysisType analysisType,
        IReadOnlyList<CustomSkill> activeSkills,
        string? userProvince = null,
        CancellationToken cancellationToken = default)
    {
        if (!_client.IsLiveMode())
        {
            return new DocumentAnalysisResult
            {
                Summary = "Resumen simulado del documento.",
                Risks = "Riesgos simulados del documento.",
                Recommendations = "Recomendaciones simuladas del documento.",
                References = "Referencias simuladas."
            };
        }

        var systemPrompt = BuildAnalysisSystemPrompt(analysisType, activeSkills, userProvince);
        var messages = new[]
        {
            new { role = "user", content = $"Analizá el siguiente documento según el derecho argentino:\n\n{documentText}" }
        };

        var raw = await _client.SendAsync(systemPrompt, messages, cancellationToken: cancellationToken);
        return ParseAnalysisResult(raw);
    }

    public async Task<StructuredTaskPlan> CreateStructuredTaskPlanAsync(
        string description,
        IReadOnlyList<Message> previousMessages,
        IReadOnlyList<CustomSkill> activeSkills,
        IReadOnlyList<ChatDocumentContext>? chatDocuments = null,
        string? userProvince = null,
        CancellationToken cancellationToken = default)
    {
        if (!_client.IsLiveMode())
            return TaskPlanParser.BuildMockPlan(description);

        var systemPrompt =
            ArgentineLegalPrompt.Build(userProvince) +
            "\n\nDado un encargo del usuario, generá un plan de trabajo legal estructurado bajo derecho argentino. " +
            "Los pasos deben anclarse en artículos de la Constitución Nacional cuando corresponda, y en la legislación argentina aplicable. " +
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
        string? userProvince = null,
        CancellationToken cancellationToken = default)
    {
        if (!_client.IsLiveMode())
        {
            var delayMs = 0;
            int.TryParse(_configuration["AI:TaskStepDelayMilliseconds"], out delayMs);
            if (delayMs > 0)
                await Task.Delay(delayMs, cancellationToken);

            return $"[Paso {step.Order} simulado: {step.Title}]\n\n" +
                   $"Resultado de desarrollo para: {step.Description}\n\n" +
                   "Configurá AI:UseMock=false y una API key de OpenAI para ejecutar con ChatGPT.";
        }

        var systemPrompt =
            ArgentineLegalPrompt.Build(userProvince) +
            "\n\nEstás ejecutando UN paso de un plan de trabajo legal en Argentina. " +
            "Entregá un resultado profesional, concreto y accionable. " +
            "Citá artículos de la Constitución Nacional y normas argentinas aplicables a este paso. " +
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
            parts.Add(
                "Documentos y contexto del caso/chat (fuente de hechos; usá montos, fechas y cláusulas concretas):\n" +
                string.Join("\n\n", chatDocuments.Select(d => $"### {d.Title}\n{d.Content}")));
        }

        var skills = activeSkills.ToList();
        if (skills.Count > 0)
        {
            parts.Add("Skills aplicadas:\n" + string.Join("\n",
                skills.Select(s => $"- {s.Name}: {s.Instructions}")));
        }

        return parts.Count == 0 ? string.Empty : string.Join("\n\n", parts);
    }

    private static string BuildChatSystemPrompt(
        IReadOnlyList<CustomSkill> activeSkills,
        IReadOnlyList<ChatDocumentContext>? chatDocuments = null,
        string? userProvince = null)
    {
        var basePrompt = ArgentineLegalPrompt.Build(userProvince);

        if (chatDocuments is { Count: > 0 })
        {
            var documentBlocks = chatDocuments.Select(d =>
                $"### {d.Title}\n{d.Content}");

            basePrompt +=
                "\n\nTenés el texto de los documentos y el contexto del caso asociados a este chat. " +
                "Son la fuente de hechos: montos, fechas de vencimiento, pagos, cláusulas, partes e intereses. " +
                "Respondé con esos datos concretos. " +
                "Prohibido decir que no tenés acceso al expediente, al contrato o a los documentos si el texto figura abajo. " +
                "Prohibido pedir datos que ya constan en esos documentos. " +
                "Si un dato puntual no aparece, indicá exactamente qué falta.\n\n" +
                string.Join("\n\n", documentBlocks);
        }

        if (!activeSkills.Any())
            return basePrompt;

        var skillInstructions = activeSkills
            .Select(s =>
                $"## Skill: {s.Name}\n" +
                $"Cuándo usarla: {s.WhenToUse}\n" +
                $"Instrucciones: {s.Instructions}\n" +
                $"Ejemplos: {s.Examples}\n" +
                $"Alertas: {s.RedFlags}\n" +
                $"Formato de salida: {s.OutputFormat}");

        return basePrompt +
               "\n\nLas siguientes skills personalizadas están aplicadas a este chat. " +
               "Tenés que seguirlas, siempre dentro del marco jurídico argentino:\n\n" +
               string.Join("\n\n", skillInstructions);
    }

    private static string BuildAnalysisSystemPrompt(
        DocumentAnalysisType type,
        IReadOnlyList<CustomSkill> activeSkills,
        string? userProvince = null)
    {
        var typeInstruction = type switch
        {
            DocumentAnalysisType.Summary =>
                "Redactá un resumen estructurado del documento bajo derecho argentino.",
            DocumentAnalysisType.RiskAnalysis =>
                "Identificá y explicá los riesgos jurídicos del documento en Argentina, con anclaje constitucional cuando corresponda.",
            DocumentAnalysisType.Recommendations =>
                "Brindá recomendaciones jurídicas accionables según la normativa argentina aplicable.",
            DocumentAnalysisType.ContractReview =>
                "Revisá el contrato destacando cláusulas, obligaciones y objeciones a la luz del CCyC y la Constitución Nacional.",
            DocumentAnalysisType.Custom =>
                "Analizá el documento en profundidad según las skills activas y el derecho argentino.",
            _ => "Analizá el documento según el derecho argentino."
        };

        var basePrompt =
            ArgentineLegalPrompt.Build(userProvince) +
            "\n\n" +
            $"{typeInstruction} " +
            "En \"references\" citá artículos de la Constitución Nacional y, si aplica, códigos o leyes argentinas. " +
            "Respondé ÚNICAMENTE con un objeto JSON con estas claves exactas: " +
            "\"summary\", \"risks\", \"recommendations\", \"references\". " +
            "Cada valor debe ser un string (podés usar viñetas con saltos de línea). " +
            "No incluyas texto fuera del objeto JSON.";

        if (!activeSkills.Any())
            return basePrompt;

        var skillInstructions = activeSkills
            .Select(s => $"- {s.Name}: {s.Instructions}");

        return basePrompt + "\n\nAplicá estas skills personalizadas, siempre bajo derecho argentino:\n" +
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

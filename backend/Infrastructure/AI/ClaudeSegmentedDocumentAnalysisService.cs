using System.Text.Json;
using JurisApp.Application.Interfaces.AI;
using JurisApp.Application.Models.Segmentation;
using JurisApp.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JurisApp.Infrastructure.AI;

public sealed class ClaudeSegmentedDocumentAnalysisService : ISegmentedDocumentAnalysisService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly AnthropicMessageClient _client;
    private readonly ClaudeOptions _options;
    private readonly ILogger<ClaudeSegmentedDocumentAnalysisService> _logger;

    public ClaudeSegmentedDocumentAnalysisService(
        AnthropicMessageClient client,
        IOptions<ClaudeOptions> options,
        ILogger<ClaudeSegmentedDocumentAnalysisService> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<SegmentedDocumentAnalysisResult> AnalyzeAsync(
        string input,
        DocumentClassificationResult classification,
        DocumentCategoryDefinition categoryDefinition,
        IReadOnlyList<CustomSkill> activeSkills,
        CancellationToken cancellationToken = default)
    {
        if (!_client.IsLiveMode())
        {
            _logger.LogInformation("Claude no configurado; devolviendo análisis segmentado simulado.");
            return SegmentedAnalysisParser.BuildMock(classification, categoryDefinition);
        }

        var systemPrompt = BuildSystemPrompt(classification, categoryDefinition, activeSkills);
        var userPrompt = BuildUserPrompt(input, classification, categoryDefinition);

        var messages = new[] { new { role = "user", content = userPrompt } };
        var raw = await _client.SendAsync(
            systemPrompt,
            messages,
            _options.SegmentedAnalysisMaxTokens,
            cancellationToken);

        try
        {
            await ClaudeDebugResponseWriter.SaveSegmentedAnalysisResponseAsync(
                raw,
                classification.CategoryKey,
                cancellationToken);
            _logger.LogInformation(
                "Respuesta cruda de Claude guardada en debug/claude-responses (categoría: {CategoryKey})",
                classification.CategoryKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo guardar la respuesta de Claude para debug.");
        }

        return SegmentedAnalysisParser.Parse(raw, classification, categoryDefinition);
    }

    private static string BuildSystemPrompt(
        DocumentClassificationResult classification,
        DocumentCategoryDefinition categoryDefinition,
        IReadOnlyList<CustomSkill> activeSkills)
    {
        var basePrompt =
            "Actuás como JurisApp, un asistente jurídico profesional para abogados argentinos.\n" +
            "Tu tarea es analizar el documento, caso o consulta del usuario según la categoría ya detectada y completar los segmentos indicados.\n" +
            "No respondas como chat libre.\n" +
            "No devuelvas markdown.\n" +
            "No agregues texto fuera del JSON.\n" +
            "Devolvé únicamente JSON válido.\n\n" +
            "Reglas:\n" +
            "1. Generá contenido para cada segmento recibido.\n" +
            "2. Respetá exactamente las keys de los segmentos.\n" +
            "3. No inventes información que no esté en el documento o caso.\n" +
            "4. Si algo no surge del texto, indicá que no se encuentra informado.\n" +
            "5. Si un segmento no aplica, devolvelo igual con una aclaración breve.\n" +
            "6. Priorizá utilidad práctica para abogados.\n" +
            "7. Detectá riesgos concretos, no abstractos.\n" +
            "8. Separá hechos, riesgos, recomendaciones y próximos pasos.\n" +
            "9. Cuando el segmento sea countable, completá itemsCount y un array items.\n" +
            "10. Cuando el segmento no sea countable, itemsCount puede ser null y items puede ser vacío.\n" +
            "11. Usá severity con uno de estos valores: \"low\", \"medium\", \"high\", \"critical\", \"neutral\".\n" +
            "12. Terminá con suggestedActions útiles para que JurisApp pueda generar tareas posteriores.\n\n" +
            $"Categoría detectada: {classification.CategoryKey} - {classification.DisplayName}\n" +
            $"Confianza: {classification.Confidence}\n";

        if (!activeSkills.Any(s => s.IsActive))
            return basePrompt;

        var skillInstructions = activeSkills
            .Where(s => s.IsActive)
            .Select(s => $"- {s.Name}: {s.Instructions}");

        return basePrompt +
               "\nAplicá estas custom skills activas:\n" +
               string.Join("\n", skillInstructions);
    }

    private static string BuildUserPrompt(
        string input,
        DocumentClassificationResult classification,
        DocumentCategoryDefinition categoryDefinition)
    {
        var mainFieldsJson = JsonSerializer.Serialize(classification.MainFields, SerializerOptions);
        var segmentsJson = JsonSerializer.Serialize(categoryDefinition.Segments, SerializerOptions);

        return
            "Campos principales detectados:\n" +
            mainFieldsJson + "\n\n" +
            "Segmentos que debés completar:\n" +
            segmentsJson + "\n\n" +
            "Formato exacto esperado:\n" +
            "{\n" +
            $"  \"categoryKey\": \"{classification.CategoryKey}\",\n" +
            $"  \"displayName\": \"{classification.DisplayName}\",\n" +
            $"  \"confidence\": {classification.Confidence},\n" +
            $"  \"mainFields\": {mainFieldsJson},\n" +
            "  \"segments\": [ { \"key\": \"\", \"title\": \"\", \"countable\": true, \"itemsCount\": 0, \"severity\": \"neutral\", \"content\": \"\", \"items\": [] } ],\n" +
            "  \"suggestedActions\": [ { \"key\": \"\", \"title\": \"\" } ]\n" +
            "}\n\n" +
            "Documento/caso/consulta original:\n" +
            input;
    }
}

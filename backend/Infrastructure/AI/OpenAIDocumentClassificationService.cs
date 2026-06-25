using System.Text;
using System.Text.Json;
using JurisApp.Application.Interfaces.AI;
using JurisApp.Application.Interfaces.Segmentation;
using JurisApp.Application.Models.Segmentation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JurisApp.Infrastructure.AI;

public sealed class OpenAIDocumentClassificationService : IDocumentClassificationService
{
    private static readonly string[] LegalKeywords =
    [
        "demanda", "contrato", "despido", "indemnización", "jurisdicción", "actor", "demandado",
        "cláusula", "plazo", "notificación", "carta documento", "convenio", "deuda", "consumidor",
        "sucesión", "herencia", "poder", "acta", "administrativo", "laboral", "alquiler"
    ];

    private readonly HttpClient _httpClient;
    private readonly OpenAIOptions _options;
    private readonly IDocumentSegmentationCatalog _catalog;
    private readonly ILogger<OpenAIDocumentClassificationService> _logger;

    public OpenAIDocumentClassificationService(
        HttpClient httpClient,
        IOptions<OpenAIOptions> options,
        IDocumentSegmentationCatalog catalog,
        ILogger<OpenAIDocumentClassificationService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _catalog = catalog;
        _logger = logger;
    }

    public async Task<DocumentClassificationResult> ClassifyAsync(
        string input,
        CancellationToken cancellationToken = default)
    {
        if (!IsLiveMode())
        {
            _logger.LogInformation("OpenAI no configurado; usando clasificación heurística.");
            return BuildHeuristicFallback(input);
        }

        try
        {
            var prompt = BuildClassificationPrompt(input);
            var raw = await SendChatCompletionAsync(prompt, cancellationToken);
            var parsed = ParseClassification(raw);
            return await NormalizeClassificationAsync(parsed, input, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falló la clasificación con OpenAI; usando fallback heurístico.");
            return BuildHeuristicFallback(input);
        }
    }

    private bool IsLiveMode() =>
        _options.Enabled && !string.IsNullOrWhiteSpace(_options.ApiKey);

    private static string BuildClassificationPrompt(string input) =>
        """
        Actuás como agente clasificador de JurisApp, una aplicación para abogados argentinos.

        Tu tarea es analizar el contenido enviado por el usuario y clasificarlo en una sola categoría.

        Categorías válidas:
        - contrato_servicios
        - contrato_laboral
        - contrato_alquiler
        - carta_documento
        - demanda
        - contestacion_demanda
        - convenio_pago
        - reclamo_deuda
        - defensa_consumidor
        - despido
        - accidente_laboral
        - sucesion
        - sociedad_poder_acta
        - documento_administrativo
        - consulta_juridica_general
        - pregunta_libre

        Reglas:
        1. Devolvé solo JSON válido.
        2. No agregues explicación fuera del JSON.
        3. Elegí una sola categoría.
        4. Si hay un documento formal, priorizá el tipo documental.
        5. Si hay un caso jurídico narrado pero no un documento formal, usá consulta_juridica_general.
        6. Si es una pregunta abierta o no jurídica/documental, usá pregunta_libre.
        7. Incluí campos principales detectables según el tipo de documento/caso.
        8. Si no se puede detectar un campo, no lo inventes.
        9. La confianza debe ir de 0 a 1.

        Formato exacto de respuesta:

        {
          "categoryKey": "",
          "displayName": "",
          "confidence": 0.0,
          "reason": "",
          "mainFields": {}
        }

        Contenido a clasificar:
        """ + input;

    private async Task<string> SendChatCompletionAsync(string prompt, CancellationToken cancellationToken)
    {
        var body = new
        {
            model = _options.Model,
            max_tokens = _options.MaxTokens,
            response_format = new { type = "json_object" },
            messages = new[]
            {
                new { role = "system", content = "Respondé únicamente con JSON válido." },
                new { role = "user", content = prompt }
            }
        };

        var json = JsonSerializer.Serialize(body);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("chat/completions", content, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "OpenAI error → Status: {StatusCode}, Body: {Body}",
                (int)response.StatusCode,
                responseBody);
            throw new InvalidOperationException($"OpenAI respondió con error {(int)response.StatusCode}.");
        }

        using var doc = JsonDocument.Parse(responseBody);
        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;
    }

    private static DocumentClassificationResult ParseClassification(string raw)
    {
        var json = JsonResponseHelper.StripMarkdownJson(raw);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var confidence = 0m;
        if (root.TryGetProperty("confidence", out var confidenceElement))
        {
            if (confidenceElement.ValueKind == JsonValueKind.Number &&
                confidenceElement.TryGetDecimal(out var parsed))
            {
                confidence = JsonResponseHelper.ClampConfidence(parsed);
            }
        }

        return new DocumentClassificationResult
        {
            CategoryKey = JsonResponseHelper.ReadString(root, "categoryKey"),
            DisplayName = JsonResponseHelper.ReadString(root, "displayName"),
            Confidence = confidence,
            Reason = JsonResponseHelper.ReadString(root, "reason"),
            MainFields = JsonResponseHelper.ReadMainFields(root)
        };
    }

    private async Task<DocumentClassificationResult> NormalizeClassificationAsync(
        DocumentClassificationResult parsed,
        string input,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(parsed.CategoryKey) || !_catalog.IsValidCategoryKey(parsed.CategoryKey))
        {
            var fallback = BuildHeuristicFallback(input);
            fallback.MainFields = parsed.MainFields.Count > 0 ? parsed.MainFields : fallback.MainFields;
            if (!string.IsNullOrWhiteSpace(parsed.Reason))
                fallback.Reason = $"Categoría inválida devuelta por OpenAI. {parsed.Reason}";

            return await EnrichDisplayNameAsync(fallback, cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(parsed.DisplayName))
            parsed = await EnrichDisplayNameAsync(parsed, cancellationToken);

        parsed.Confidence = JsonResponseHelper.ClampConfidence(parsed.Confidence);
        return parsed;
    }

    private async Task<DocumentClassificationResult> EnrichDisplayNameAsync(
        DocumentClassificationResult result,
        CancellationToken cancellationToken)
    {
        var category = await _catalog.GetByCategoryKeyAsync(result.CategoryKey, cancellationToken);
        if (category is not null && string.IsNullOrWhiteSpace(result.DisplayName))
            result.DisplayName = category.DisplayName;

        return result;
    }

    private DocumentClassificationResult BuildHeuristicFallback(string input)
    {
        var trimmed = input.Trim();
        var isShort = trimmed.Length < 120;
        var hasLegalKeyword = LegalKeywords.Any(k =>
            trimmed.Contains(k, StringComparison.OrdinalIgnoreCase));

        var categoryKey = isShort && !hasLegalKeyword
            ? "pregunta_libre"
            : "consulta_juridica_general";

        var category = _catalog.GetByCategoryKeyAsync(categoryKey).GetAwaiter().GetResult()
            ?? new DocumentCategoryDefinition { DisplayName = categoryKey };

        return new DocumentClassificationResult
        {
            CategoryKey = categoryKey,
            DisplayName = category.DisplayName,
            Confidence = 0.3m,
            Reason = "Clasificación heurística por fallback (OpenAI no disponible o respuesta inválida).",
            MainFields = new Dictionary<string, object>()
        };
    }
}

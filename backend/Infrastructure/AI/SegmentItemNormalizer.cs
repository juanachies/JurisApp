using System.Text.Json;
using JurisApp.Application.Interfaces.AI;

namespace JurisApp.Infrastructure.AI;

internal static class SegmentItemNormalizer
{
    private static readonly string[] TitleKeys =
    [
        "title", "parte", "cuota", "plazo", "supuesto", "penalidad", "causal",
        "aspecto", "formalidad", "riesgo", "documento", "recomendacion", "paso", "nota"
    ];

    private static readonly string[] DescriptionKeys =
    [
        "description", "descripcion", "obligacion", "condicion", "monto"
    ];

    private static readonly string[] MetaKeys =
    [
        "title", "description", "descripcion", "severity", "recommendation", "recomendacion"
    ];

    public static DocumentAnalysisSegmentItemResult FromJson(JsonElement itemElement)
    {
        var properties = ReadStringProperties(itemElement);

        var title = FirstValue(properties, TitleKeys) ?? string.Empty;
        var description = FirstValue(properties, DescriptionKeys) ?? string.Empty;
        var recommendation = FirstValue(properties, "recommendation", "recomendacion") ?? string.Empty;
        var severity = JsonResponseHelper.NormalizeSeverity(
            FirstValue(properties, "severity") ?? "neutral");

        var detailParts = properties
            .Where(p => !MetaKeys.Contains(p.Key, StringComparer.OrdinalIgnoreCase)
                        && !TitleKeys.Contains(p.Key, StringComparer.OrdinalIgnoreCase)
                        && !DescriptionKeys.Contains(p.Key, StringComparer.OrdinalIgnoreCase))
            .Select(p => $"{FormatLabel(p.Key)}: {p.Value}")
            .ToList();

        if (detailParts.Count > 0)
        {
            description = string.IsNullOrWhiteSpace(description)
                ? string.Join(" · ", detailParts)
                : $"{description} · {string.Join(" · ", detailParts)}";
        }

        if (string.IsNullOrWhiteSpace(title) && properties.Count > 0)
        {
            var first = properties.First();
            title = first.Value;
            if (string.IsNullOrWhiteSpace(description) && properties.Count > 1)
            {
                description = string.Join(
                    " · ",
                    properties.Skip(1).Select(p => $"{FormatLabel(p.Key)}: {p.Value}"));
            }
        }

        return new DocumentAnalysisSegmentItemResult
        {
            Title = title,
            Description = description,
            Severity = severity,
            Recommendation = recommendation
        };
    }

    private static Dictionary<string, string> ReadStringProperties(JsonElement element)
    {
        var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (element.ValueKind != JsonValueKind.Object)
            return properties;

        foreach (var property in element.EnumerateObject())
        {
            var value = ReadScalarAsString(property.Value);
            if (!string.IsNullOrWhiteSpace(value))
                properties[property.Name] = value;
        }

        return properties;
    }

    private static string ReadScalarAsString(JsonElement element) =>
        element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number => element.ToString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => element.ToString()
        };

    private static string? FirstValue(IReadOnlyDictionary<string, string> properties, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (properties.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                return value;
        }

        return null;
    }

    private static string FormatLabel(string key) =>
        key.Replace('_', ' ');
}

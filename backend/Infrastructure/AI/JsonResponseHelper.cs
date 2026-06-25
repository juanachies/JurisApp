using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace JurisApp.Infrastructure.AI;

internal static class JsonResponseHelper
{
    public static string StripMarkdownJson(string raw)
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

    public static JsonDocument ParseJsonDocument(string raw)
    {
        var json = StripMarkdownJson(raw);

        try
        {
            return JsonDocument.Parse(json);
        }
        catch (JsonException)
        {
            return JsonDocument.Parse(SalvageTruncatedJson(json));
        }
    }

    private static string SalvageTruncatedJson(string json)
    {
        var suggestedActionsIndex = json.LastIndexOf("\"suggestedActions\"", StringComparison.Ordinal);
        if (suggestedActionsIndex > 0)
        {
            var withoutActions = json[..suggestedActionsIndex].TrimEnd().TrimEnd(',');
            try
            {
                using var _ = JsonDocument.Parse(withoutActions + "\n}");
                return withoutActions + "\n}";
            }
            catch (JsonException)
            {
                // Continue with bracket balancing below.
            }
        }

        var builder = new StringBuilder(json.TrimEnd());
        var openBraces = json.Count(c => c == '{');
        var closeBraces = json.Count(c => c == '}');
        var openBrackets = json.Count(c => c == '[');
        var closeBrackets = json.Count(c => c == ']');

        for (var i = 0; i < openBrackets - closeBrackets; i++)
            builder.Append(']');

        for (var i = 0; i < openBraces - closeBraces; i++)
            builder.Append('}');

        return builder.ToString();
    }

    public static string ReadString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
            return string.Empty;

        return value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : value.ToString();
    }

    public static string ReadJsonField(JsonElement root, string propertyName)
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

    public static Dictionary<string, object> ReadMainFields(JsonElement root)
    {
        var fields = new Dictionary<string, object>();

        if (!root.TryGetProperty("mainFields", out var mainFields) ||
            mainFields.ValueKind != JsonValueKind.Object)
        {
            return fields;
        }

        foreach (var property in mainFields.EnumerateObject())
        {
            fields[property.Name] = property.Value.ValueKind switch
            {
                JsonValueKind.String => property.Value.GetString() ?? string.Empty,
                JsonValueKind.Number => property.Value.TryGetDecimal(out var d) ? d : property.Value.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Array => property.Value.EnumerateArray()
                    .Select(e => e.ValueKind == JsonValueKind.String ? e.GetString() ?? string.Empty : e.ToString())
                    .ToList(),
                _ => property.Value.ToString()
            };
        }

        return fields;
    }

    public static decimal ClampConfidence(decimal value) =>
        value < 0 ? 0 : value > 1 ? 1 : value;

    public static string NormalizeSeverity(string? severity)
    {
        var normalized = (severity ?? "neutral").Trim().ToLowerInvariant();
        return normalized switch
        {
            "low" or "medium" or "high" or "critical" or "neutral" => normalized,
            _ => "neutral"
        };
    }
}

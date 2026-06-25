using System.Text.Json;
using JurisApp.Application.Interfaces.AI;
using JurisApp.Application.Models.Segmentation;

namespace JurisApp.Infrastructure.AI;

internal static class SegmentedAnalysisParser
{
    public static SegmentedDocumentAnalysisResult Parse(
        string raw,
        DocumentClassificationResult classification,
        DocumentCategoryDefinition categoryDefinition)
    {
        try
        {
            using var doc = JsonResponseHelper.ParseJsonDocument(raw);
            var root = doc.RootElement;

            var result = new SegmentedDocumentAnalysisResult
            {
                CategoryKey = string.IsNullOrWhiteSpace(JsonResponseHelper.ReadString(root, "categoryKey"))
                    ? classification.CategoryKey
                    : JsonResponseHelper.ReadString(root, "categoryKey"),
                DisplayName = string.IsNullOrWhiteSpace(JsonResponseHelper.ReadString(root, "displayName"))
                    ? classification.DisplayName
                    : JsonResponseHelper.ReadString(root, "displayName"),
                Confidence = ReadConfidence(root, classification.Confidence),
                MainFields = ReadMainFieldsOrFallback(root, classification),
                SuggestedActions = ReadSuggestedActions(root)
            };

            result.Segments = ReconcileSegments(root, categoryDefinition);
            return result;
        }
        catch
        {
            return BuildFallback(classification, categoryDefinition);
        }
    }

    public static SegmentedDocumentAnalysisResult BuildMock(
        DocumentClassificationResult classification,
        DocumentCategoryDefinition categoryDefinition)
    {
        return new SegmentedDocumentAnalysisResult
        {
            CategoryKey = classification.CategoryKey,
            DisplayName = classification.DisplayName,
            Confidence = classification.Confidence,
            MainFields = classification.MainFields,
            Segments = categoryDefinition.Segments.Select(segment => new DocumentAnalysisSegmentResult
            {
                Key = segment.Key,
                Title = segment.Title,
                Countable = segment.Countable,
                ItemsCount = segment.Countable ? 0 : null,
                Severity = "neutral",
                Content = "Análisis simulado en modo desarrollo.",
                Items = []
            }).ToList(),
            SuggestedActions =
            [
                new SuggestedActionResult
                {
                    Key = "revisar_analisis",
                    Title = "Revisar análisis simulado"
                }
            ]
        };
    }

    private static SegmentedDocumentAnalysisResult BuildFallback(
        DocumentClassificationResult classification,
        DocumentCategoryDefinition categoryDefinition)
    {
        return new SegmentedDocumentAnalysisResult
        {
            CategoryKey = classification.CategoryKey,
            DisplayName = classification.DisplayName,
            Confidence = classification.Confidence,
            MainFields = classification.MainFields,
            Segments = categoryDefinition.Segments.Select(segment => new DocumentAnalysisSegmentResult
            {
                Key = segment.Key,
                Title = segment.Title,
                Countable = segment.Countable,
                ItemsCount = segment.Countable ? 0 : null,
                Severity = "neutral",
                Content = "No se detectó información suficiente para completar este segmento.",
                Items = []
            }).ToList(),
            SuggestedActions = []
        };
    }

    private static decimal ReadConfidence(JsonElement root, decimal fallback)
    {
        if (!root.TryGetProperty("confidence", out var confidenceElement))
            return fallback;

        if (confidenceElement.ValueKind == JsonValueKind.Number &&
            confidenceElement.TryGetDecimal(out var parsed))
        {
            return JsonResponseHelper.ClampConfidence(parsed);
        }

        return fallback;
    }

    private static Dictionary<string, object> ReadMainFieldsOrFallback(
        JsonElement root,
        DocumentClassificationResult classification)
    {
        var fields = JsonResponseHelper.ReadMainFields(root);
        return fields.Count > 0 ? fields : classification.MainFields;
    }

    private static List<DocumentAnalysisSegmentResult> ReconcileSegments(
        JsonElement root,
        DocumentCategoryDefinition categoryDefinition)
    {
        var parsedByKey = new Dictionary<string, DocumentAnalysisSegmentResult>();

        if (root.TryGetProperty("segments", out var segmentsElement) &&
            segmentsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var segmentElement in segmentsElement.EnumerateArray())
            {
                var key = JsonResponseHelper.ReadString(segmentElement, "key");
                if (string.IsNullOrWhiteSpace(key))
                    continue;

                parsedByKey[key] = ParseSegment(segmentElement);
            }
        }

        return categoryDefinition.Segments.Select(definition =>
        {
            if (parsedByKey.TryGetValue(definition.Key, out var parsed))
            {
                parsed.Title = string.IsNullOrWhiteSpace(parsed.Title) ? definition.Title : parsed.Title;
                parsed.Countable = definition.Countable;
                return parsed;
            }

            return new DocumentAnalysisSegmentResult
            {
                Key = definition.Key,
                Title = definition.Title,
                Countable = definition.Countable,
                ItemsCount = definition.Countable ? 0 : null,
                Severity = "neutral",
                Content = "No se detectó información suficiente para este segmento según el documento.",
                Items = []
            };
        }).ToList();
    }

    private static DocumentAnalysisSegmentResult ParseSegment(JsonElement segmentElement)
    {
        var items = new List<DocumentAnalysisSegmentItemResult>();
        if (segmentElement.TryGetProperty("items", out var itemsElement) &&
            itemsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var itemElement in itemsElement.EnumerateArray())
            {
                items.Add(SegmentItemNormalizer.FromJson(itemElement));
            }
        }

        int? itemsCount = null;
        if (segmentElement.TryGetProperty("itemsCount", out var countElement) &&
            countElement.ValueKind == JsonValueKind.Number &&
            countElement.TryGetInt32(out var count))
        {
            itemsCount = count;
        }

        return new DocumentAnalysisSegmentResult
        {
            Key = JsonResponseHelper.ReadString(segmentElement, "key"),
            Title = JsonResponseHelper.ReadString(segmentElement, "title"),
            Countable = segmentElement.TryGetProperty("countable", out var countable) &&
                        countable.ValueKind == JsonValueKind.True,
            ItemsCount = itemsCount ?? (items.Count > 0 ? items.Count : null),
            Severity = JsonResponseHelper.NormalizeSeverity(JsonResponseHelper.ReadString(segmentElement, "severity")),
            Content = JsonResponseHelper.ReadString(segmentElement, "content"),
            Items = items
        };
    }

    private static List<SuggestedActionResult> ReadSuggestedActions(JsonElement root)
    {
        var actions = new List<SuggestedActionResult>();

        if (!root.TryGetProperty("suggestedActions", out var actionsElement) ||
            actionsElement.ValueKind != JsonValueKind.Array)
        {
            return actions;
        }

        foreach (var actionElement in actionsElement.EnumerateArray())
        {
            actions.Add(new SuggestedActionResult
            {
                Key = JsonResponseHelper.ReadString(actionElement, "key"),
                Title = JsonResponseHelper.ReadString(actionElement, "title")
            });
        }

        return actions;
    }
}

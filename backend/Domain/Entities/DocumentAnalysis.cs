using System.Text.Json;
using JurisApp.Domain.Common;
using JurisApp.Domain.Enums;

namespace JurisApp.Domain.Entities;

public class DocumentAnalysis : BaseEntity
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Guid DocumentId { get; private set; }
    public string Summary { get; private set; } = string.Empty;
    public string Risks { get; private set; } = string.Empty;
    public string Recommendations { get; private set; } = string.Empty;
    public string References { get; private set; } = string.Empty;
    public DocumentAnalysisType Type { get; private set; }
    public bool IsSegmented { get; private set; }
    public string? CategoryKey { get; private set; }
    public string? CategoryDisplayName { get; private set; }
    public decimal? Confidence { get; private set; }
    public string? MainFieldsJson { get; private set; }
    public string? SegmentsJson { get; private set; }
    public string? SuggestedActionsJson { get; private set; }

    public Document Document { get; private set; } = null!;

    protected DocumentAnalysis() { }

    public DocumentAnalysis(
        Guid id,
        Guid documentId,
        string summary,
        string risks,
        string recommendations,
        string references,
        DocumentAnalysisType type)
        : base(id)
    {
        DocumentId = documentId;
        Summary = summary;
        Risks = risks;
        Recommendations = recommendations;
        References = references;
        Type = type;
        IsSegmented = false;
    }

    public static DocumentAnalysis CreateSegmented(
        Guid id,
        Guid documentId,
        string categoryKey,
        string categoryDisplayName,
        decimal confidence,
        string mainFieldsJson,
        string segmentsJson,
        string suggestedActionsJson,
        string summary,
        string risks,
        string recommendations,
        string references)
    {
        return new DocumentAnalysis
        {
            Id = id,
            DocumentId = documentId,
            CategoryKey = categoryKey,
            CategoryDisplayName = categoryDisplayName,
            Confidence = confidence,
            MainFieldsJson = mainFieldsJson,
            SegmentsJson = segmentsJson,
            SuggestedActionsJson = suggestedActionsJson,
            Summary = summary,
            Risks = risks,
            Recommendations = recommendations,
            References = references,
            Type = DocumentAnalysisType.Segmented,
            IsSegmented = true
        };
    }

    public Dictionary<string, object> DeserializeMainFields()
    {
        if (string.IsNullOrWhiteSpace(MainFieldsJson))
            return new Dictionary<string, object>();

        return JsonSerializer.Deserialize<Dictionary<string, object>>(MainFieldsJson, JsonOptions)
            ?? new Dictionary<string, object>();
    }

    public List<DocumentAnalysisSegmentPayload> DeserializeSegments()
    {
        if (string.IsNullOrWhiteSpace(SegmentsJson))
            return [];

        return JsonSerializer.Deserialize<List<DocumentAnalysisSegmentPayload>>(SegmentsJson, JsonOptions)
            ?? [];
    }

    public List<DocumentAnalysisSuggestedActionPayload> DeserializeSuggestedActions()
    {
        if (string.IsNullOrWhiteSpace(SuggestedActionsJson))
            return [];

        return JsonSerializer.Deserialize<List<DocumentAnalysisSuggestedActionPayload>>(SuggestedActionsJson, JsonOptions)
            ?? [];
    }
}

public sealed class DocumentAnalysisSegmentPayload
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool Countable { get; set; }
    public int? ItemsCount { get; set; }
    public string Severity { get; set; } = "neutral";
    public string Content { get; set; } = string.Empty;
    public List<DocumentAnalysisSegmentItemPayload> Items { get; set; } = new();
}

public sealed class DocumentAnalysisSegmentItemPayload
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "neutral";
    public string Recommendation { get; set; } = string.Empty;
}

public sealed class DocumentAnalysisSuggestedActionPayload
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

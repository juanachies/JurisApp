using JurisApp.Application.DTOs.Analysis;
using JurisApp.Application.Interfaces.AI;

namespace JurisApp.Application.Mappings;

public static class AnalysisMappings
{
    public static SegmentedDocumentAnalysisDto ToDto(
        this SegmentedDocumentAnalysisResult result,
        Guid? id = null,
        Guid? documentId = null) => new()
    {
        Id = id,
        DocumentId = documentId,
        CategoryKey = result.CategoryKey,
        DisplayName = result.DisplayName,
        Confidence = result.Confidence,
        MainFields = result.MainFields,
        Segments = result.Segments.Select(s => s.ToDto()).ToList(),
        SuggestedActions = result.SuggestedActions.Select(a => a.ToDto()).ToList()
    };

    private static DocumentAnalysisSegmentDto ToDto(this DocumentAnalysisSegmentResult segment) => new()
    {
        Key = segment.Key,
        Title = segment.Title,
        Countable = segment.Countable,
        ItemsCount = segment.ItemsCount,
        Severity = segment.Severity,
        Content = segment.Content,
        Items = segment.Items.Select(i => i.ToDto()).ToList()
    };

    private static DocumentAnalysisSegmentItemDto ToDto(this DocumentAnalysisSegmentItemResult item) => new()
    {
        Title = item.Title,
        Description = item.Description,
        Severity = item.Severity,
        Recommendation = item.Recommendation
    };

    private static SuggestedActionDto ToDto(this SuggestedActionResult action) => new()
    {
        Key = action.Key,
        Title = action.Title
    };
}

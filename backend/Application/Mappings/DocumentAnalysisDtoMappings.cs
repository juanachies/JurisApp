using JurisApp.Application.DTOs.Analysis;
using JurisApp.Application.Interfaces.Services;
using JurisApp.Application.Mappings;
using JurisApp.Domain.Entities;

namespace JurisApp.Application.Mappings;

public static class DocumentAnalysisDtoMappings
{
    public static SegmentedDocumentAnalysisDto ToSegmentedDto(this DocumentAnalysis analysis)
    {
        var segments = analysis.DeserializeSegments().Select(segment => new DocumentAnalysisSegmentDto
        {
            Key = segment.Key,
            Title = segment.Title,
            Countable = segment.Countable,
            ItemsCount = segment.ItemsCount,
            Severity = segment.Severity,
            Content = segment.Content,
            Items = segment.Items.Select(item => new DocumentAnalysisSegmentItemDto
            {
                Title = item.Title,
                Description = item.Description,
                Severity = item.Severity,
                Recommendation = item.Recommendation
            }).ToList()
        }).ToList();

        var actions = analysis.DeserializeSuggestedActions().Select(action => new SuggestedActionDto
        {
            Key = action.Key,
            Title = action.Title
        }).ToList();

        return new SegmentedDocumentAnalysisDto
        {
            Id = analysis.Id,
            DocumentId = analysis.DocumentId,
            CategoryKey = analysis.CategoryKey ?? string.Empty,
            DisplayName = analysis.CategoryDisplayName ?? string.Empty,
            Confidence = analysis.Confidence ?? 0,
            MainFields = analysis.DeserializeMainFields(),
            Segments = segments,
            SuggestedActions = actions
        };
    }
}

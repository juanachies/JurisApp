using JurisApp.Application.DTOs.Documents;
using JurisApp.Domain.Entities;

namespace JurisApp.Application.Mappings;

public static class DocumentMappings
{
    public static DocumentDto ToDto(this Document document) => new()
    {
        Id = document.Id,
        ChatId = document.ChatId,
        FolderId = document.FolderId,
        Title = document.Title,
        Url = document.Url
    };

    public static DocumentAnalysisDto ToDto(this DocumentAnalysis analysis) => new()
    {
        Id = analysis.Id,
        DocumentId = analysis.DocumentId,
        Type = analysis.Type,
        Summary = analysis.Summary,
        Risks = analysis.Risks,
        Recommendations = analysis.Recommendations,
        References = analysis.References
    };
}

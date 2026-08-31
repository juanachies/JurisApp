using JurisApp.Domain.Common;
using JurisApp.Domain.Enums;

namespace JurisApp.Domain.Entities;

public class DocumentAnalysis : BaseEntity
{
    public Guid DocumentId { get; private set; }
    public string Summary { get; private set; } = string.Empty;
    public string Risks { get; private set; } = string.Empty;
    public string Recommendations { get; private set; } = string.Empty;
    public string References { get; private set; } = string.Empty;
    public DocumentAnalysisType Type { get; private set; }

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
    }

    public void ApplyPartial(
        DocumentAnalysisType type,
        string? summary,
        string? risks,
        string? recommendations,
        string? references)
    {
        Type = type;
        if (summary is not null)
            Summary = summary;
        if (risks is not null)
            Risks = risks;
        if (recommendations is not null)
            Recommendations = recommendations;
        if (references is not null)
            References = references;
        Touch();
    }
}

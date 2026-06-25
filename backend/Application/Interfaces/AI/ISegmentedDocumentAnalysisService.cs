using JurisApp.Application.Models.Segmentation;
using JurisApp.Domain.Entities;

namespace JurisApp.Application.Interfaces.AI;

public interface ISegmentedDocumentAnalysisService
{
    Task<SegmentedDocumentAnalysisResult> AnalyzeAsync(
        string input,
        DocumentClassificationResult classification,
        DocumentCategoryDefinition categoryDefinition,
        IReadOnlyList<CustomSkill> activeSkills,
        CancellationToken cancellationToken = default);
}

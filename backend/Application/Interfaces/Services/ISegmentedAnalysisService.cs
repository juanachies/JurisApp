using JurisApp.Application.Common;
using JurisApp.Application.DTOs.Analysis;

namespace JurisApp.Application.Interfaces.Services;

public interface ISegmentedAnalysisService
{
    Task<Result<SegmentedDocumentAnalysisDto>> AnalyzeAsync(
        Guid userId,
        AnalyzeSegmentedRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<SegmentedDocumentAnalysisDto>> GetByDocumentIdAsync(
        Guid userId,
        Guid documentId,
        CancellationToken cancellationToken = default);
}

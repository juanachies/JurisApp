using JurisApp.Application.Common;
using JurisApp.Application.DTOs.Documents;

namespace JurisApp.Application.Services.Interfaces;

public interface IDocumentService
{
    Task<Result<DocumentDto>> UploadAsync(Guid userId, UploadDocumentRequest request, CancellationToken cancellationToken = default);
    Task<Result<DocumentAnalysisDto>> AnalyzeAsync(Guid userId, AnalyzeDocumentRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<DocumentDto>>> GetByChatIdAsync(Guid userId, Guid chatId, CancellationToken cancellationToken = default);
}

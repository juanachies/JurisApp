using JurisApp.Domain.Entities;

namespace JurisApp.Application.Interfaces.Persistence;

public interface IDocumentAnalysisRepository
{
    Task<DocumentAnalysis?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DocumentAnalysis?> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task AddAsync(DocumentAnalysis documentAnalysis, CancellationToken cancellationToken = default);
    void Update(DocumentAnalysis documentAnalysis);
}

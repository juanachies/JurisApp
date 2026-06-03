using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JurisApp.Infrastructure.Persistence.Repositories;

public class DocumentAnalysisRepository : IDocumentAnalysisRepository
{
    private readonly AppDbContext _context;

    public DocumentAnalysisRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DocumentAnalysis?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.DocumentAnalyses.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<DocumentAnalysis?> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
        => await _context.DocumentAnalyses.FirstOrDefaultAsync(a => a.DocumentId == documentId, cancellationToken);

    public async Task AddAsync(DocumentAnalysis documentAnalysis, CancellationToken cancellationToken = default)
        => await _context.DocumentAnalyses.AddAsync(documentAnalysis, cancellationToken);

    public void Update(DocumentAnalysis documentAnalysis)
        => _context.DocumentAnalyses.Update(documentAnalysis);
}
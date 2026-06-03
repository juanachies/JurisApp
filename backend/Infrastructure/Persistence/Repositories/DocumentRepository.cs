using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JurisApp.Infrastructure.Persistence.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _context;

    public DocumentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Documents
            .Include(d => d.Analysis)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Document>> GetByChatIdAsync(Guid chatId, CancellationToken cancellationToken = default)
        => await _context.Documents
            .Where(d => d.ChatId == chatId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Document>> GetByFolderIdAsync(Guid folderId, CancellationToken cancellationToken = default)
        => await _context.Documents
            .Where(d => d.FolderId == folderId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Document document, CancellationToken cancellationToken = default)
        => await _context.Documents.AddAsync(document, cancellationToken);

    public void Update(Document document)
        => _context.Documents.Update(document);

    public void Delete(Document document)
        => _context.Documents.Remove(document);
}
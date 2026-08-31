using JurisApp.Domain.Entities;

namespace JurisApp.Application.Interfaces.Persistence;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Document>> GetByChatIdAsync(Guid chatId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Document>> GetByFolderIdAsync(Guid folderId, CancellationToken cancellationToken = default);
    Task AddAsync(Document document, CancellationToken cancellationToken = default);
    void Update(Document document);
    void Delete(Document document);
    Task<int> CountOwnedByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

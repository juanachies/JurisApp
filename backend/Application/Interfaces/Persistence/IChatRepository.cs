using JurisApp.Domain.Entities;

namespace JurisApp.Application.Interfaces.Persistence;

public interface IChatRepository
{
    Task<Chat?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Chat?> GetByIdLightAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Chat>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Chat chat, CancellationToken cancellationToken = default);
    void Update(Chat chat);
    void Delete(Chat chat);
}

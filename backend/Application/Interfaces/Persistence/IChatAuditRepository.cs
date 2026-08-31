using JurisApp.Domain.Entities;

namespace JurisApp.Application.Interfaces.Persistence;

public interface IChatAuditRepository
{
    Task<ChatAudit?> GetByChatIdAsync(Guid chatId, CancellationToken cancellationToken = default);
    Task AddAsync(ChatAudit audit, CancellationToken cancellationToken = default);
    void Update(ChatAudit audit);
}

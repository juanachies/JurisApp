using JurisApp.Domain.Entities;

namespace JurisApp.Application.Interfaces.Persistence;

public interface IMessageRepository
{
    Task<IReadOnlyList<Message>> GetByChatIdAsync(Guid chatId, CancellationToken cancellationToken = default);
    Task AddAsync(Message message, CancellationToken cancellationToken = default);
}

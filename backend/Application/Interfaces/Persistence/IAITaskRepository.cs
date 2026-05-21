using JurisApp.Domain.Entities;

namespace JurisApp.Application.Interfaces.Persistence;

public interface IAITaskRepository
{
    Task<AITask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AITask>> GetByChatIdAsync(Guid chatId, CancellationToken cancellationToken = default);
    Task AddAsync(AITask aiTask, CancellationToken cancellationToken = default);
    void Update(AITask aiTask);
}

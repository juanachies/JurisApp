using JurisApp.Domain.Entities;

namespace JurisApp.Application.Interfaces.Persistence;

public interface IAITaskRepository
{
    Task<AITask?> GetByIdWithStepsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AITask>> GetByChatIdWithStepsAsync(Guid chatId, CancellationToken cancellationToken = default);
    Task AddAsync(AITask aiTask, CancellationToken cancellationToken = default);
    void Update(AITask aiTask);
    Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

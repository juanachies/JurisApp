namespace JurisApp.Application.Interfaces.Services;

public interface IAITaskExecutionQueue
{
    ValueTask EnqueueAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<(Guid UserId, Guid TaskId)> ReadAllAsync(CancellationToken cancellationToken);
}

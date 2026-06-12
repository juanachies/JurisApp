using JurisApp.Application.Common;
using JurisApp.Application.DTOs.AITasks;

namespace JurisApp.Application.Interfaces.Services;

public interface IAITaskService
{
    Task<Result<AITaskDto>> CreateAsync(Guid userId, CreateAITaskRequest request, CancellationToken cancellationToken = default);
    Task<Result<AITaskDto>> CompleteAsync(Guid userId, Guid taskId, string result, CancellationToken cancellationToken = default);
    Task<Result<AITaskDto>> GetByIdAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default);
    Task<Result<AITaskDto>> CancelAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<AITaskDto>>> GetByChatIdAsync(Guid userId, Guid chatId, CancellationToken cancellationToken = default);
}

using JurisApp.Application.Common;
using JurisApp.Application.DTOs.AITasks;

namespace JurisApp.Application.Services.Interfaces;

public interface IAITaskService
{
    Task<Result<AITaskDto>> CreateAsync(Guid userId, CreateAITaskRequest request, CancellationToken cancellationToken = default);
    Task<Result<AITaskDto>> CompleteAsync(Guid userId, Guid taskId, string result, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<AITaskDto>>> GetByChatIdAsync(Guid userId, Guid chatId, CancellationToken cancellationToken = default);
}

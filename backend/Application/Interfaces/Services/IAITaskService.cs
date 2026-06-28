using JurisApp.Application.Common;
using JurisApp.Application.DTOs.AITasks;

namespace JurisApp.Application.Interfaces.Services;

public interface IAITaskService
{
    Task<Result<AITaskDetailDto>> CreateAsync(Guid userId, CreateAITaskRequest request, CancellationToken cancellationToken = default);
    Task<Result<AITaskDetailDto>> GetByIdAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<AITaskDetailDto>>> GetByChatIdAsync(Guid userId, Guid chatId, CancellationToken cancellationToken = default);
    Task<Result<AITaskDetailDto>> UpdatePlanAsync(Guid userId, Guid taskId, UpdateAITaskPlanRequest request, CancellationToken cancellationToken = default);
    Task<Result<AITaskDetailDto>> ApprovePlanAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default);
    Task<Result<AITaskDetailDto>> ExecuteNextStepAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default);
    Task<Result<AITaskDetailDto>> PauseAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default);
    Task<Result<AITaskDetailDto>> ResumeAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default);
    Task<Result<AITaskDetailDto>> CancelAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default);
}

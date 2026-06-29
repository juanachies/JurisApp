using JurisApp.Application.DTOs.AITasks;
using JurisApp.Domain.Entities;

namespace JurisApp.Application.Mappings;

public static class AITaskMappings
{
    public static AITaskDto ToDto(this AITask task) => new()
    {
        Id = task.Id,
        ChatId = task.ChatId,
        Description = task.Description,
        Status = task.Status,
        Plan = task.Plan,
        Result = task.Result,
        CurrentStepIndex = task.CurrentStepIndex,
        IsPaused = task.IsPaused,
        Steps = task.Steps
            .OrderBy(s => s.Order)
            .Select(s => s.ToDto())
            .ToList()
    };

    public static TaskStepDto ToDto(this AITaskStep step) => new()
    {
        Id = step.Id,
        Order = step.Order,
        Title = step.Title,
        Description = step.Description,
        Status = step.Status,
        Result = step.Result
    };
}

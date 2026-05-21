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
        Result = task.Result
    };
}

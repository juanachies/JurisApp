using JurisApp.Domain.Enums;

namespace JurisApp.Application.DTOs.AITasks;

public class AITaskDetailDto
{
    public Guid Id { get; set; }
    public Guid ChatId { get; set; }
    public string Description { get; set; } = string.Empty;
    public AITaskStatus Status { get; set; }
    public string Plan { get; set; } = string.Empty;
    public string? Result { get; set; }
    public int CurrentStepIndex { get; set; }
    public bool IsPaused { get; set; }
    public IReadOnlyList<TaskStepDto> Steps { get; set; } = Array.Empty<TaskStepDto>();
}

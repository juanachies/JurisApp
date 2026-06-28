using JurisApp.Domain.Enums;

namespace JurisApp.Application.DTOs.AITasks;

public class TaskStepDto
{
    public Guid Id { get; set; }
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AITaskStepStatus Status { get; set; }
    public string? Result { get; set; }
}

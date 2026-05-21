using JurisApp.Domain.Enums;

namespace JurisApp.Application.DTOs.AITasks;

public class AITaskDto
{
    public Guid Id { get; set; }
    public Guid ChatId { get; set; }
    public string Description { get; set; } = string.Empty;
    public AITaskStatus Status { get; set; }
    public string Plan { get; set; } = string.Empty;
    public string? Result { get; set; }
}

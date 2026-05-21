namespace JurisApp.Application.DTOs.AITasks;

public class CreateAITaskRequest
{
    public Guid ChatId { get; set; }
    public string Description { get; set; } = string.Empty;
}

namespace JurisApp.Application.DTOs.AITasks;

public class UpdateAITaskPlanRequest
{
    public List<UpdateTaskStepRequest> Steps { get; set; } = [];
}

public class UpdateTaskStepRequest
{
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

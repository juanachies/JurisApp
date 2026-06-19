namespace JurisApp.Application.Interfaces.AI;

public class StructuredTaskPlan
{
    public string Objective { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<StructuredTaskStep> Steps { get; set; } = [];
}

public class StructuredTaskStep
{
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

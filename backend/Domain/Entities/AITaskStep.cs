using JurisApp.Domain.Common;
using JurisApp.Domain.Enums;

namespace JurisApp.Domain.Entities;

public class AITaskStep : BaseEntity
{
    public Guid AITaskId { get; private set; }
    public int Order { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public AITaskStepStatus Status { get; private set; }
    public string? Result { get; private set; }

    public AITask AITask { get; private set; } = null!;

    protected AITaskStep() { }

    public AITaskStep(Guid id, Guid aiTaskId, int order, string title, string description)
        : base(id)
    {
        AITaskId = aiTaskId;
        Order = order;
        Title = title;
        Description = description;
        Status = AITaskStepStatus.Pending;
    }

    public void UpdateContent(string title, string description)
    {
        Title = title;
        Description = description;
        Touch();
    }

    public void MarkAsInProgress()
    {
        Status = AITaskStepStatus.InProgress;
        Touch();
    }

    public void MarkAsCompleted(string result)
    {
        Status = AITaskStepStatus.Completed;
        Result = result;
        Touch();
    }

    public void MarkAsFailed(string result)
    {
        Status = AITaskStepStatus.Failed;
        Result = result;
        Touch();
    }
}

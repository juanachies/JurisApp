using JurisApp.Domain.Common;
using JurisApp.Domain.Enums;

namespace JurisApp.Domain.Entities;

public class AITask : BaseEntity
{
    public Guid ChatId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public AITaskStatus Status { get; private set; }
    public string Plan { get; private set; } = string.Empty;
    public string? Result { get; private set; }

    public Chat Chat { get; private set; } = null!;

    protected AITask() { }

    public AITask(Guid id, Guid chatId, string description, string plan)
        : base(id)
    {
        ChatId = chatId;
        Description = description;
        Plan = plan;
        Status = AITaskStatus.Pending;
    }

    public void MarkAsInProgress()
    {
        Status = AITaskStatus.InProgress;
        Touch();
    }

    public void MarkAsCompleted(string result)
    {
        Status = AITaskStatus.Completed;
        Result = result;
        Touch();
    }

    public void MarkAsFailed(string result)
    {
        Status = AITaskStatus.Failed;
        Result = result;
        Touch();
    }

    public void Cancel()
    {
        Status = AITaskStatus.Cancelled;
        Touch();
    }
}

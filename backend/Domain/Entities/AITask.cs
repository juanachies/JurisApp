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
    public int CurrentStepIndex { get; private set; }
    public bool IsPaused { get; private set; }

    public Chat Chat { get; private set; } = null!;
    public ICollection<AITaskStep> Steps { get; private set; } = new List<AITaskStep>();

    protected AITask() { }

    public AITask(Guid id, Guid chatId, string description, string plan)
        : base(id)
    {
        ChatId = chatId;
        Description = description;
        Plan = plan;
        Status = AITaskStatus.AwaitingApproval;
        CurrentStepIndex = 0;
    }

    public void SetPlanSummary(string plan)
    {
        Plan = plan;
        Touch();
    }

    public void ApprovePlan()
    {
        Status = AITaskStatus.InProgress;
        CurrentStepIndex = 1;
        IsPaused = false;
        Touch();
    }

    public void Pause()
    {
        IsPaused = true;
        Touch();
    }

    public void Resume()
    {
        IsPaused = false;
        Touch();
    }

    public void AdvanceToNextStep()
    {
        CurrentStepIndex++;
        Touch();
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

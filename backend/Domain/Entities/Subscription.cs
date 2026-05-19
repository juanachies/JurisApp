using JurisApp.Domain.Common;
using JurisApp.Domain.Enums;

namespace JurisApp.Domain.Entities;

public class Subscription : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid PlanId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public SubscriptionStatus Status { get; private set; }

    public User User { get; private set; } = null!;
    public Plan Plan { get; private set; } = null!;

    protected Subscription() { }

    public Subscription(Guid id, Guid userId, Guid planId, DateTime startDate)
        : base(id)
    {
        UserId = userId;
        PlanId = planId;
        StartDate = startDate;
        Status = SubscriptionStatus.Active;
    }

    public void Cancel()
    {
        Status = SubscriptionStatus.Cancelled;
        EndDate = DateTime.UtcNow;
        Touch();
    }

    public void Expire()
    {
        Status = SubscriptionStatus.Expired;
        EndDate ??= DateTime.UtcNow;
        Touch();
    }

    public bool IsActive() =>
        Status == SubscriptionStatus.Active &&
        (EndDate == null || EndDate > DateTime.UtcNow);
}

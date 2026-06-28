using JurisApp.Domain.Enums;

namespace JurisApp.Application.DTOs.Plans;

public class CurrentPlanDto
{
    public Guid PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public PlanType PlanType { get; set; }
    public decimal Price { get; set; }
    public string LimitsJson { get; set; } = string.Empty;
    public bool HasActiveSubscription { get; set; }
    public SubscriptionStatus? SubscriptionStatus { get; set; }
    public DateTime? StartDate { get; set; }
}

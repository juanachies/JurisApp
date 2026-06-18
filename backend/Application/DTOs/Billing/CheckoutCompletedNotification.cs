namespace JurisApp.Application.DTOs.Billing;

public class CheckoutCompletedNotification
{
    public Guid UserId { get; set; }
    public Guid PlanId { get; set; }
    public string StripeCustomerId { get; set; } = string.Empty;
    public string StripeSubscriptionId { get; set; } = string.Empty;
}

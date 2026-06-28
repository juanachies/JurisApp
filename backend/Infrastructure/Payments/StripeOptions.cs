namespace JurisApp.Infrastructure.Payments;

public class StripeOptions
{
    public const string SectionName = "Stripe";

    public string? SecretKey { get; set; }
    public string? WebhookSecret { get; set; }
    public string? SuccessUrl { get; set; }
    public string? CancelUrl { get; set; }
    public bool UseMock { get; set; }
}

using JurisApp.Application.Common;
using JurisApp.Application.DTOs.Billing;

namespace JurisApp.Application.Interfaces.Payments;

public interface IPaymentService
{
    Task<Result<string>> CreateCheckoutSessionAsync(
        Guid userId,
        Guid planId,
        CancellationToken cancellationToken = default);

    Task<Result<CheckoutCompletedNotification?>> ParseCheckoutCompletedWebhookAsync(
        string json,
        string stripeSignature,
        CancellationToken cancellationToken = default);
}

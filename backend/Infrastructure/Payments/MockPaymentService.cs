using JurisApp.Application.Common;
using JurisApp.Application.DTOs.Billing;
using JurisApp.Application.Interfaces.Payments;
using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Domain.Enums;

namespace JurisApp.Infrastructure.Payments;

public class MockPaymentService : IPaymentService
{
    private readonly IPlanRepository _planRepository;

    public MockPaymentService(IPlanRepository planRepository)
    {
        _planRepository = planRepository;
    }

    public async Task<Result<string>> CreateCheckoutSessionAsync(
        Guid userId,
        Guid planId,
        CancellationToken cancellationToken = default)
    {
        var plan = await _planRepository.GetByIdAsync(planId, cancellationToken);
        if (plan is null)
            return Result<string>.Failure(Error.NotFound("Plan not found."));

        if (plan.Type == PlanType.Free)
            return Result<string>.Failure(Error.Validation("Free plan does not require payment."));

        return Result<string>.Failure(Error.Validation(
            "Stripe mock mode is enabled. Use POST /api/billing/simulate-purchase instead."));
    }

    public Task<Result<CheckoutCompletedNotification?>> ParseCheckoutCompletedWebhookAsync(
        string json,
        string stripeSignature,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Result<CheckoutCompletedNotification?>.Success(null));
}

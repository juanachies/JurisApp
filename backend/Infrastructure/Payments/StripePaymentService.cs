using JurisApp.Application.Common;
using JurisApp.Application.DTOs.Billing;
using JurisApp.Application.Interfaces.Payments;
using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Domain.Enums;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace JurisApp.Infrastructure.Payments;

public class StripePaymentService : IPaymentService
{
    private readonly IPlanRepository _planRepository;
    private readonly StripeOptions _options;

    public StripePaymentService(IPlanRepository planRepository, IOptions<StripeOptions> options)
    {
        _planRepository = planRepository;
        _options = options.Value;
        StripeConfiguration.ApiKey = _options.SecretKey;
    }

    public async Task<Result<string>> CreateCheckoutSessionAsync(
        Guid userId,
        Guid planId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.SecretKey))
            return Result<string>.Failure(Error.Validation("Stripe:SecretKey is not configured."));

        if (string.IsNullOrWhiteSpace(_options.SuccessUrl) || string.IsNullOrWhiteSpace(_options.CancelUrl))
            return Result<string>.Failure(Error.Validation("Stripe SuccessUrl and CancelUrl must be configured."));

        var plan = await _planRepository.GetByIdAsync(planId, cancellationToken);
        if (plan is null)
            return Result<string>.Failure(Error.NotFound("Plan not found."));

        if (plan.Type == PlanType.Free)
            return Result<string>.Failure(Error.Validation("Free plan does not require payment."));

        if (string.IsNullOrWhiteSpace(plan.StripePriceId))
            return Result<string>.Failure(Error.Validation("Plan does not have a Stripe price configured."));

        var sessionOptions = new SessionCreateOptions
        {
            Mode = "subscription",
            SuccessUrl = AppendCheckoutSessionId(_options.SuccessUrl!),
            CancelUrl = _options.CancelUrl,
            ClientReferenceId = userId.ToString(),
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Price = plan.StripePriceId,
                    Quantity = 1
                }
            ],
            Metadata = new Dictionary<string, string>
            {
                ["userId"] = userId.ToString(),
                ["planId"] = planId.ToString()
            }
        };

        try
        {
            var service = new SessionService();
            var session = await service.CreateAsync(sessionOptions, cancellationToken: cancellationToken);

            if (string.IsNullOrWhiteSpace(session.Url))
                return Result<string>.Failure(Error.ExternalService("Stripe did not return a checkout URL."));

            return Result<string>.Success(session.Url);
        }
        catch (StripeException ex)
        {
            return Result<string>.Failure(Error.ExternalService($"Stripe error: {ex.Message}"));
        }
    }

    public Task<Result<CheckoutCompletedNotification?>> ParseCheckoutCompletedWebhookAsync(
        string json,
        string stripeSignature,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.WebhookSecret))
            return Task.FromResult(Result<CheckoutCompletedNotification?>.Failure(
                Error.Validation("Stripe:WebhookSecret is not configured.")));

        if (string.IsNullOrWhiteSpace(stripeSignature))
            return Task.FromResult(Result<CheckoutCompletedNotification?>.Failure(
                Error.Validation("Missing Stripe-Signature header.")));

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, _options.WebhookSecret);
        }
        catch (StripeException ex)
        {
            return Task.FromResult(Result<CheckoutCompletedNotification?>.Failure(
                Error.Validation($"Invalid webhook signature: {ex.Message}")));
        }

        if (stripeEvent.Type != EventTypes.CheckoutSessionCompleted)
            return Task.FromResult(Result<CheckoutCompletedNotification?>.Success(null));

        if (stripeEvent.Data.Object is not Session session)
            return Task.FromResult(Result<CheckoutCompletedNotification?>.Failure(
                Error.Validation("Invalid checkout session payload.")));

        if (!session.Metadata.TryGetValue("userId", out var userIdStr) ||
            !Guid.TryParse(userIdStr, out var userId))
        {
            return Task.FromResult(Result<CheckoutCompletedNotification?>.Failure(
                Error.Validation("Missing or invalid userId in checkout session metadata.")));
        }

        if (!session.Metadata.TryGetValue("planId", out var planIdStr) ||
            !Guid.TryParse(planIdStr, out var planId))
        {
            return Task.FromResult(Result<CheckoutCompletedNotification?>.Failure(
                Error.Validation("Missing or invalid planId in checkout session metadata.")));
        }

        if (string.IsNullOrWhiteSpace(session.CustomerId))
        {
            return Task.FromResult(Result<CheckoutCompletedNotification?>.Failure(
                Error.Validation("Missing CustomerId in checkout session.")));
        }

        if (string.IsNullOrWhiteSpace(session.SubscriptionId))
        {
            return Task.FromResult(Result<CheckoutCompletedNotification?>.Failure(
                Error.Validation("Missing SubscriptionId in checkout session.")));
        }

        var notification = new CheckoutCompletedNotification
        {
            UserId = userId,
            PlanId = planId,
            StripeCustomerId = session.CustomerId,
            StripeSubscriptionId = session.SubscriptionId
        };

        return Task.FromResult(Result<CheckoutCompletedNotification?>.Success(notification));
    }

    public async Task<Result<CheckoutCompletedNotification?>> FindPaidCheckoutAsync(
        Guid userId,
        string? sessionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.SecretKey))
            return Result<CheckoutCompletedNotification?>.Failure(
                Error.Validation("Stripe:SecretKey is not configured."));

        try
        {
            var service = new SessionService();
            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                var session = await service.GetAsync(sessionId, cancellationToken: cancellationToken);
                return MapPaidSession(session, userId, strict: true);
            }

            var list = await service.ListAsync(
                new SessionListOptions { Limit = 100, Status = "complete" },
                cancellationToken: cancellationToken);

            foreach (var session in list.Data)
            {
                if (!BelongsToUser(session, userId))
                    continue;

                var mapped = MapPaidSession(session, userId, strict: false);
                if (mapped.IsSuccess && mapped.Value is not null)
                    return mapped;
            }

            return Result<CheckoutCompletedNotification?>.Success(null);
        }
        catch (StripeException ex)
        {
            return Result<CheckoutCompletedNotification?>.Failure(
                Error.ExternalService($"Stripe error: {ex.Message}"));
        }
    }

    internal static string AppendCheckoutSessionId(string successUrl)
    {
        if (successUrl.Contains("{CHECKOUT_SESSION_ID}", StringComparison.Ordinal))
            return successUrl;

        var separator = successUrl.Contains('?') ? "&" : "?";
        return successUrl + separator + "session_id={CHECKOUT_SESSION_ID}";
    }

    private static Result<CheckoutCompletedNotification?> MapPaidSession(Session session, Guid userId, bool strict)
    {
        if (!BelongsToUser(session, userId))
            return Result<CheckoutCompletedNotification?>.Failure(
                Error.Validation("El pago no corresponde a esta cuenta."));

        var completed = string.Equals(session.Status, "complete", StringComparison.OrdinalIgnoreCase);
        var paid = string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase)
            || string.Equals(session.PaymentStatus, "no_payment_required", StringComparison.OrdinalIgnoreCase);

        if (!completed || !paid)
        {
            return strict
                ? Result<CheckoutCompletedNotification?>.Failure(
                    Error.Validation("El pago todavía no figura como confirmado."))
                : Result<CheckoutCompletedNotification?>.Success(null);
        }

        if (session.Metadata is null
            || !session.Metadata.TryGetValue("planId", out var planIdStr)
            || !Guid.TryParse(planIdStr, out var planId))
        {
            return strict
                ? Result<CheckoutCompletedNotification?>.Failure(
                    Error.Validation("El pago no tiene el plan asociado."))
                : Result<CheckoutCompletedNotification?>.Success(null);
        }

        if (string.IsNullOrWhiteSpace(session.CustomerId) || string.IsNullOrWhiteSpace(session.SubscriptionId))
        {
            return strict
                ? Result<CheckoutCompletedNotification?>.Failure(
                    Error.Validation("El pago no tiene la suscripción de Stripe."))
                : Result<CheckoutCompletedNotification?>.Success(null);
        }

        return Result<CheckoutCompletedNotification?>.Success(new CheckoutCompletedNotification
        {
            UserId = userId,
            PlanId = planId,
            StripeCustomerId = session.CustomerId,
            StripeSubscriptionId = session.SubscriptionId
        });
    }

    private static bool BelongsToUser(Session session, Guid userId)
    {
        if (string.Equals(session.ClientReferenceId, userId.ToString(), StringComparison.OrdinalIgnoreCase))
            return true;

        return session.Metadata is not null
            && session.Metadata.TryGetValue("userId", out var raw)
            && Guid.TryParse(raw, out var metadataUserId)
            && metadataUserId == userId;
    }
}

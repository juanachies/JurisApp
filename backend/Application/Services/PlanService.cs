using JurisApp.Application.Common;
using JurisApp.Application.DTOs.Plans;
using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Application.Interfaces.Services;
using JurisApp.Domain.Entities;
using JurisApp.Domain.Enums;

namespace JurisApp.Application.Services;

public class PlanService : IPlanService
{
    private readonly IPlanRepository _planRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PlanService(
        IPlanRepository planRepository,
        ISubscriptionRepository subscriptionRepository,
        IUnitOfWork unitOfWork)
    {
        _planRepository = planRepository;
        _subscriptionRepository = subscriptionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<PlanDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var plans = await _planRepository.GetAllAsync(cancellationToken);

        var dtos = plans.Select(p => new PlanDto
        {
            Id         = p.Id,
            Name       = p.Name,
            Type       = p.Type,
            Price      = p.Price,
            LimitsJson = p.LimitsJson
        }).ToList();

        return Result<IReadOnlyList<PlanDto>>.Success(dtos);
    }

    public async Task<Result<SubscriptionDto>> SubscribeAsync(
        Guid userId,
        Guid planId,
        CancellationToken cancellationToken = default)
    {
        var plan = await _planRepository.GetByIdAsync(planId, cancellationToken);
        if (plan is null)
            return Result<SubscriptionDto>.Failure(Error.NotFound("Plan not found."));

        if (plan.Type != PlanType.Free)
            return Result<SubscriptionDto>.Failure(Error.Validation("Paid plans require Stripe checkout."));

        var existing = await _subscriptionRepository.GetActiveByUserIdAsync(userId, cancellationToken);
        if (existing is not null)
            return Result<SubscriptionDto>.Failure(Error.Conflict("User already has an active subscription."));

        var subscription = new Subscription(Guid.NewGuid(), userId, planId, DateTime.UtcNow);
        await _subscriptionRepository.AddAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<SubscriptionDto>.Success(MapToDto(subscription));
    }

    public async Task<Result<CurrentPlanDto>> GetCurrentPlanAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetActiveByUserIdAsync(userId, cancellationToken);
        if (subscription is not null)
        {
            var plan = await _planRepository.GetByIdAsync(subscription.PlanId, cancellationToken);
            if (plan is null)
                return Result<CurrentPlanDto>.Failure(Error.NotFound("Plan not found."));

            return Result<CurrentPlanDto>.Success(new CurrentPlanDto
            {
                PlanId                  = plan.Id,
                PlanName                = plan.Name,
                PlanType                = plan.Type,
                Price                   = plan.Price,
                LimitsJson              = plan.LimitsJson,
                HasActiveSubscription   = true,
                SubscriptionStatus      = subscription.Status,
                StartDate               = subscription.StartDate
            });
        }

        var freePlan = await _planRepository.GetByTypeAsync(PlanType.Free, cancellationToken);
        if (freePlan is null)
            return Result<CurrentPlanDto>.Failure(Error.NotFound("Free plan not found."));

        return Result<CurrentPlanDto>.Success(new CurrentPlanDto
        {
            PlanId                = freePlan.Id,
            PlanName              = freePlan.Name,
            PlanType              = freePlan.Type,
            Price                 = freePlan.Price,
            LimitsJson            = freePlan.LimitsJson,
            HasActiveSubscription = false
        });
    }

    public async Task<Result<SubscriptionDto>> ActivatePaidSubscriptionAsync(
        Guid userId,
        Guid planId,
        string stripeCustomerId,
        string stripeSubscriptionId,
        CancellationToken cancellationToken = default)
    {
        var plan = await _planRepository.GetByIdAsync(planId, cancellationToken);
        if (plan is null)
            return Result<SubscriptionDto>.Failure(Error.NotFound("Plan not found."));

        if (plan.Type == PlanType.Free)
            return Result<SubscriptionDto>.Failure(Error.Validation("Free plan does not require payment."));

        var existingByStripe = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(
            stripeSubscriptionId, cancellationToken);
        if (existingByStripe is not null && existingByStripe.IsActive())
            return Result<SubscriptionDto>.Success(MapToDto(existingByStripe));

        var active = await _subscriptionRepository.GetActiveByUserIdAsync(userId, cancellationToken);
        if (active is not null)
        {
            active.Cancel();
            _subscriptionRepository.Update(active);
        }

        Subscription subscription;
        if (existingByStripe is not null)
        {
            subscription = existingByStripe;
            subscription.ActivateFromPayment(planId, stripeCustomerId, stripeSubscriptionId);
            _subscriptionRepository.Update(subscription);
        }
        else
        {
            subscription = new Subscription(Guid.NewGuid(), userId, planId, DateTime.UtcNow);
            subscription.ActivateFromPayment(planId, stripeCustomerId, stripeSubscriptionId);
            await _subscriptionRepository.AddAsync(subscription, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<SubscriptionDto>.Success(MapToDto(subscription));
    }

    private static SubscriptionDto MapToDto(Subscription subscription) => new()
    {
        Id        = subscription.Id,
        UserId    = subscription.UserId,
        PlanId    = subscription.PlanId,
        StartDate = subscription.StartDate,
        EndDate   = subscription.EndDate,
        Status    = subscription.Status
    };
}
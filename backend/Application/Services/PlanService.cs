using JurisApp.Application.Common;
using JurisApp.Application.DTOs.Plans;
using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Application.Interfaces.Services;
using JurisApp.Domain.Entities;

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

        var existing = await _subscriptionRepository.GetActiveByUserIdAsync(userId, cancellationToken);
        if (existing is not null)
            return Result<SubscriptionDto>.Failure(Error.Conflict("User already has an active subscription."));

        var subscription = new Subscription(Guid.NewGuid(), userId, planId, DateTime.UtcNow);
        await _subscriptionRepository.AddAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<SubscriptionDto>.Success(new SubscriptionDto
        {
            Id        = subscription.Id,
            UserId    = subscription.UserId,
            PlanId    = subscription.PlanId,
            StartDate = subscription.StartDate,
            EndDate   = subscription.EndDate,
            Status    = subscription.Status
        });
    }

    public async Task<Result<SubscriptionDto>> GetActiveSubscriptionAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetActiveByUserIdAsync(userId, cancellationToken);
        if (subscription is null)
            return Result<SubscriptionDto>.Failure(Error.NotFound("No active subscription found."));

        return Result<SubscriptionDto>.Success(new SubscriptionDto
        {
            Id        = subscription.Id,
            UserId    = subscription.UserId,
            PlanId    = subscription.PlanId,
            StartDate = subscription.StartDate,
            EndDate   = subscription.EndDate,
            Status    = subscription.Status
        });
    }
}
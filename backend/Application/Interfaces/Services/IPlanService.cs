using JurisApp.Application.Common;
using JurisApp.Application.DTOs.Plans;
using JurisApp.Domain.Enums;

namespace JurisApp.Application.Interfaces.Services;

public interface IPlanService
{
    Task<Result<IReadOnlyList<PlanDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<SubscriptionDto>> SubscribeAsync(Guid userId, Guid planId, CancellationToken cancellationToken = default);
    Task<Result<CurrentPlanDto>> GetCurrentPlanAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<SubscriptionDto>> ChangePlanAsync(Guid userId, Guid planId, CancellationToken cancellationToken = default);
    Task<Result<SubscriptionDto>> CancelCurrentAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<PlanDto>> CreateAsync(CreatePlanRequest request, CancellationToken cancellationToken = default);
    Task<Result<PlanDto>> UpdateAsync(Guid planId, UpdatePlanRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid planId, CancellationToken cancellationToken = default);
    Task<Result<SubscriptionDto>> ActivatePaidSubscriptionAsync(
        Guid userId,
        Guid planId,
        string stripeCustomerId,
        string stripeSubscriptionId,
        CancellationToken cancellationToken = default);
}

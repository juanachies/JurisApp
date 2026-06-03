using JurisApp.Application.Common;
using JurisApp.Application.DTOs.Plans;

namespace JurisApp.Application.Interfaces.Services;

public interface IPlanService
{
    Task<Result<IReadOnlyList<PlanDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<SubscriptionDto>> SubscribeAsync(Guid userId, Guid planId, CancellationToken cancellationToken = default);
    Task<Result<SubscriptionDto>> GetActiveSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default);
}
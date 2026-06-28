using JurisApp.Application.Interfaces.Auth;
using JurisApp.Application.Interfaces.Services;
using JurisApp.Presentation.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JurisApp.Presentation.Controllers;

[ApiController]
[Route("api/plans")]
public class PlansController : ControllerBase
{
    private readonly IPlanService _planService;
    private readonly ICurrentUserService _currentUserService;

    public PlansController(IPlanService planService, ICurrentUserService currentUserService)
    {
        _planService = planService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _planService.GetAllAsync(cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("{planId:guid}/subscribe")]
    [Authorize]
    public async Task<IActionResult> Subscribe(Guid planId, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _planService.SubscribeAsync(userId, planId, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("subscription/active")]
    [Authorize]
    public async Task<IActionResult> GetActiveSubscription(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _planService.GetActiveSubscriptionAsync(userId, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("current")]
    [Authorize]
    public async Task<IActionResult> GetCurrentPlan(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _planService.GetCurrentPlanAsync(userId, cancellationToken);
        return result.ToActionResult();
    }
}
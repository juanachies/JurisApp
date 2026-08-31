using JurisApp.Application.DTOs.Plans;
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

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        [FromBody] CreatePlanRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _planService.CreateAsync(request, cancellationToken);
        if (!result.IsSuccess)
            return result.ToActionResult();

        return new ObjectResult(result.Value) { StatusCode = StatusCodes.Status201Created };
    }

    [HttpPut("{planId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        Guid planId,
        [FromBody] UpdatePlanRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _planService.UpdateAsync(planId, request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpDelete("{planId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid planId, CancellationToken cancellationToken)
    {
        var result = await _planService.DeleteAsync(planId, cancellationToken);
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

    [HttpPost("{planId:guid}/change")]
    [Authorize]
    public async Task<IActionResult> Change(Guid planId, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _planService.ChangePlanAsync(userId, planId, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("current/cancel")]
    [Authorize]
    public async Task<IActionResult> Cancel(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _planService.CancelCurrentAsync(userId, cancellationToken);
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

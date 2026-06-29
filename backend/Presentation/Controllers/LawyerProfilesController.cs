using JurisApp.Application.DTOs.LawyerProfiles;
using JurisApp.Application.Interfaces.Auth;
using JurisApp.Application.Interfaces.Services;
using JurisApp.Domain.Enums;
using JurisApp.Presentation.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JurisApp.Presentation.Controllers;

[ApiController]
[Route("api/lawyer-profiles")]
[Authorize]
public class LawyerProfilesController : ControllerBase
{
    private readonly ILawyerProfileService _lawyerProfileService;
    private readonly ICurrentUserService _currentUserService;

    public LawyerProfilesController(
        ILawyerProfileService lawyerProfileService,
        ICurrentUserService currentUserService)
    {
        _lawyerProfileService = lawyerProfileService;
        _currentUserService = currentUserService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateVerificationRequest(
        [FromBody] CreateLawyerProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _lawyerProfileService.CreateVerificationRequestAsync(userId, request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyRequest(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _lawyerProfileService.GetByUserIdAsync(userId, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyRequest(
        [FromBody] UpdateLawyerProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _lawyerProfileService.UpdateAsync(userId, request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("requests")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllRequests(
        [FromQuery] LawyerVerificationStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await _lawyerProfileService.GetAllRequestsAsync(status, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("requests/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetRequestDetail(Guid id, CancellationToken cancellationToken)
    {
        var result = await _lawyerProfileService.GetRequestDetailAsync(id, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("requests/{id:guid}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveRequest(Guid id, CancellationToken cancellationToken)
    {
        var adminUserId = _currentUserService.UserId!.Value;
        var result = await _lawyerProfileService.ApproveAsync(id, adminUserId, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("requests/{id:guid}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RejectRequest(
        Guid id,
        [FromBody] RejectLawyerRequest request,
        CancellationToken cancellationToken)
    {
        var adminUserId = _currentUserService.UserId!.Value;
        var result = await _lawyerProfileService.RejectAsync(id, adminUserId, request.Reason, cancellationToken);
        return result.ToActionResult();
    }
}

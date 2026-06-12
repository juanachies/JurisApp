using JurisApp.Application.DTOs.LawyerProfiles;
using JurisApp.Application.Interfaces.Auth;
using JurisApp.Application.Interfaces.Services;
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
    public async Task<IActionResult> Create(
        [FromBody] CreateLawyerProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _lawyerProfileService.CreateAsync(userId, request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _lawyerProfileService.GetByUserIdAsync(userId, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile(
        [FromBody] UpdateLawyerProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _lawyerProfileService.UpdateAsync(userId, request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("verify")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Verify(
        [FromBody] VerifyLawyerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _lawyerProfileService.VerifyAsync(request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(
        [FromBody] RejectLawyerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _lawyerProfileService.RejectAsync(request, cancellationToken);
        return result.ToActionResult();
    }
}
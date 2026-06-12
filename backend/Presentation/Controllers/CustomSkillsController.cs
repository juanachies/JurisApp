using JurisApp.Application.DTOs.CustomSkills;
using JurisApp.Application.Interfaces.Auth;
using JurisApp.Application.Interfaces.Services;
using JurisApp.Presentation.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JurisApp.Presentation.Controllers;

[ApiController]
[Route("api/custom-skills")]
[Authorize(Roles = "Lawyer,Admin")]
public class CustomSkillsController : ControllerBase
{
    private readonly ICustomSkillService _customSkillService;
    private readonly ILawyerProfileService _lawyerProfileService;
    private readonly ICurrentUserService _currentUserService;

    public CustomSkillsController(
        ICustomSkillService customSkillService,
        ILawyerProfileService lawyerProfileService,
        ICurrentUserService currentUserService)
    {
        _customSkillService = customSkillService;
        _lawyerProfileService = lawyerProfileService;
        _currentUserService = currentUserService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCustomSkillRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _customSkillService.CreateAsync(userId, request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateCustomSkillRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _customSkillService.UpdateAsync(userId, id, request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMySkills(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var profileResult = await _lawyerProfileService.GetByUserIdAsync(userId, cancellationToken);
        if (!profileResult.IsSuccess)
            return profileResult.ToActionResult();

        var result = await _customSkillService.GetByLawyerProfileIdAsync(
            userId,
            profileResult.Value!.Id,
            cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("lawyer-profile/{lawyerProfileId:guid}")]
    public async Task<IActionResult> GetByLawyerProfile(
        Guid lawyerProfileId,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _customSkillService.GetByLawyerProfileIdAsync(userId, lawyerProfileId, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _customSkillService.SetActiveAsync(userId, id, isActive: true, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _customSkillService.SetActiveAsync(userId, id, isActive: false, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("apply")]
    public async Task<IActionResult> ApplyToChat(
        [FromBody] ApplyCustomSkillToChatRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _customSkillService.ApplyToChatAsync(userId, request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("remove")]
    public async Task<IActionResult> RemoveFromChat(
        [FromBody] ApplyCustomSkillToChatRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _customSkillService.RemoveFromChatAsync(userId, request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _customSkillService.DeleteAsync(userId, id, cancellationToken);
        return result.ToActionResult();
    }
}
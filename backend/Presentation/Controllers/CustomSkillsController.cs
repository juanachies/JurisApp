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
    private readonly ICurrentUserService _currentUserService;

    public CustomSkillsController(
        ICustomSkillService customSkillService,
        ICurrentUserService currentUserService)
    {
        _customSkillService = customSkillService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _customSkillService.GetByUserIdAsync(userId, cancellationToken);
        return result.ToActionResult();
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

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _customSkillService.SetActiveAsync(userId, id, true, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _customSkillService.SetActiveAsync(userId, id, false, cancellationToken);
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
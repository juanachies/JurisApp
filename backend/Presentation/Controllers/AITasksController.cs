using JurisApp.Application.DTOs.AITasks;
using JurisApp.Application.Interfaces.Auth;
using JurisApp.Application.Interfaces.Services;
using JurisApp.Presentation.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JurisApp.Presentation.Controllers;

[ApiController]
[Route("api/ai-tasks")]
[Authorize]
public class AITasksController : ControllerBase
{
    private readonly IAITaskService _aiTaskService;
    private readonly ICurrentUserService _currentUserService;

    public AITasksController(
        IAITaskService aiTaskService,
        ICurrentUserService currentUserService)
    {
        _aiTaskService = aiTaskService;
        _currentUserService = currentUserService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateAITaskRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _aiTaskService.CreateAsync(userId, request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _aiTaskService.GetByIdAsync(userId, id, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _aiTaskService.CancelAsync(userId, id, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("chat/{chatId:guid}")]
    public async Task<IActionResult> GetByChat(Guid chatId, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _aiTaskService.GetByChatIdAsync(userId, chatId, cancellationToken);
        return result.ToActionResult();
    }
}
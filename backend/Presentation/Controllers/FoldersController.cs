using JurisApp.Application.DTOs.Folders;
using JurisApp.Application.Interfaces.Auth;
using JurisApp.Application.Interfaces.Services;
using JurisApp.Presentation.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JurisApp.Presentation.Controllers;

[ApiController]
[Route("api/folders")]
[Authorize(Roles = "Lawyer,Admin")]
public class FoldersController : ControllerBase
{
    private readonly IFolderService _folderService;
    private readonly ICurrentUserService _currentUserService;

    public FoldersController(
        IFolderService folderService,
        ICurrentUserService currentUserService)
    {
        _folderService = folderService;
        _currentUserService = currentUserService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateFolderRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _folderService.CreateAsync(userId, request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _folderService.GetByUserIdAsync(userId, cancellationToken);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _folderService.DeleteAsync(userId, id, cancellationToken);
        return result.ToActionResult();
    }
}
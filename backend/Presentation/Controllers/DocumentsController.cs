using JurisApp.Application.DTOs.Documents;
using JurisApp.Application.Interfaces.Auth;
using JurisApp.Application.Interfaces.Services;
using JurisApp.Presentation.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JurisApp.Presentation.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly ICurrentUserService _currentUserService;

    public DocumentsController(
        IDocumentService documentService,
        ICurrentUserService currentUserService)
    {
        _documentService = documentService;
        _currentUserService = currentUserService;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(
        [FromForm] UploadDocumentFormRequest form,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;

        if (form.File is null)
        {
            return BadRequest(new
            {
                Code = "Validation",
                Message = "El archivo es obligatorio."
            });
        }

        var request = new UploadDocumentRequest
        {
            ChatId      = form.ChatId,
            FolderId    = form.FolderId,
            Title       = form.File.FileName,
            FileName    = form.File.FileName,
            ContentType = form.File.ContentType,
            FileStream  = form.File.OpenReadStream()
        };

        var result = await _documentService.UploadAsync(userId, request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _documentService.GetByIdAsync(userId, id, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("chat/{chatId:guid}")]
    public async Task<IActionResult> GetByChat(Guid chatId, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _documentService.GetByChatIdAsync(userId, chatId, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze(
        [FromBody] AnalyzeDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _documentService.AnalyzeAsync(userId, request, cancellationToken);
        return result.ToActionResult();
    }
}

public sealed class UploadDocumentFormRequest
{
    public IFormFile? File { get; set; }
    public Guid ChatId { get; set; }
    public Guid? FolderId { get; set; }
}

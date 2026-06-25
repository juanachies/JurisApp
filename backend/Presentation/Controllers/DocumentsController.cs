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
    private readonly ISegmentedAnalysisService _segmentedAnalysisService;
    private readonly ICurrentUserService _currentUserService;

    public DocumentsController(
        IDocumentService documentService,
        ISegmentedAnalysisService segmentedAnalysisService,
        ICurrentUserService currentUserService)
    {
        _documentService = documentService;
        _segmentedAnalysisService = segmentedAnalysisService;
        _currentUserService = currentUserService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile file,
        [FromForm] Guid chatId,
        [FromForm] Guid? folderId,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;

        var request = new UploadDocumentRequest
        {
            ChatId      = chatId,
            FolderId    = folderId,
            Title       = file.FileName,
            FileName    = file.FileName,
            ContentType = file.ContentType,
            FileStream  = file.OpenReadStream()
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

    [HttpGet("{id:guid}/analysis")]
    public async Task<IActionResult> GetAnalysis(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _segmentedAnalysisService.GetByDocumentIdAsync(userId, id, cancellationToken);
        return result.ToActionResult();
    }
}
using JurisApp.Application.DTOs.Analysis;
using JurisApp.Application.Interfaces.Auth;
using JurisApp.Application.Interfaces.Services;
using JurisApp.Presentation.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JurisApp.Presentation.Controllers;

[ApiController]
[Route("api/analysis")]
[Authorize]
public class AnalysisController : ControllerBase
{
    private readonly ISegmentedAnalysisService _segmentedAnalysisService;
    private readonly ICurrentUserService _currentUserService;

    public AnalysisController(
        ISegmentedAnalysisService segmentedAnalysisService,
        ICurrentUserService currentUserService)
    {
        _segmentedAnalysisService = segmentedAnalysisService;
        _currentUserService = currentUserService;
    }

    [HttpPost("segmented")]
    public async Task<IActionResult> AnalyzeSegmented(
        [FromBody] AnalyzeSegmentedRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _segmentedAnalysisService.AnalyzeAsync(userId, request, cancellationToken);
        return result.ToActionResult();
    }
}

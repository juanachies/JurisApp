using JurisApp.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace JurisApp.Presentation.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new OkResult();

        return ToErrorActionResult(result.Error);
    }

    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(result.Value);

        return ToErrorActionResult(result.Error);
    }

    private static IActionResult ToErrorActionResult(Error error) => error.Code switch
    {
        "NotFound"        => new NotFoundObjectResult(new { error.Code, error.Message }),
        "Unauthorized"    => new UnauthorizedObjectResult(new { error.Code, error.Message }),
        "Conflict"        => new ConflictObjectResult(new { error.Code, error.Message }),
        "ExternalService" => new ObjectResult(new { error.Code, error.Message }) { StatusCode = 502 },
        _                 => new BadRequestObjectResult(new { error.Code, error.Message })
    };
}

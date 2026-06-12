using JurisApp.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace JurisApp.Presentation.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new OkResult();

        return result.Error.Code switch
        {
            "NotFound"     => new NotFoundObjectResult(new { result.Error.Code, result.Error.Message }),
            "Unauthorized" => new UnauthorizedObjectResult(new { result.Error.Code, result.Error.Message }),
            "Conflict"     => new ConflictObjectResult(new { result.Error.Code, result.Error.Message }),
            _              => new BadRequestObjectResult(new { result.Error.Code, result.Error.Message })
        };
    }

    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(result.Value);

        return result.Error.Code switch
        {
            "NotFound"     => new NotFoundObjectResult(new { result.Error.Code, result.Error.Message }),
            "Unauthorized" => new UnauthorizedObjectResult(new { result.Error.Code, result.Error.Message }),
            "Conflict"     => new ConflictObjectResult(new { result.Error.Code, result.Error.Message }),
            _              => new BadRequestObjectResult(new { result.Error.Code, result.Error.Message })
        };
    }

    public static IActionResult ToCreatedResult<T>(this Result<T> result, string routeName, object routeValues)
    {
        if (result.IsSuccess)
            return new CreatedAtRouteResult(routeName, routeValues, result.Value);

        return result.Error.Code switch
        {
            "NotFound"     => new NotFoundObjectResult(new { result.Error.Code, result.Error.Message }),
            "Unauthorized" => new UnauthorizedObjectResult(new { result.Error.Code, result.Error.Message }),
            "Conflict"     => new ConflictObjectResult(new { result.Error.Code, result.Error.Message }),
            _              => new BadRequestObjectResult(new { result.Error.Code, result.Error.Message })
        };
    }
}

using Common.Errors;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.V1.Infrastructure;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller)
    {
        if (result.IsSuccess)
            return controller.Ok(result.Value);

        var statusCode = result.Errors.Any(e => e is NotFoundError)
            ? StatusCodes.Status404NotFound
            : result.Errors.Any(e => e is ConflictError)
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status400BadRequest;

        return controller.Problem(statusCode: statusCode, detail: string.Join(" ", result.Errors.Select(e => e.Message)));
    }
}

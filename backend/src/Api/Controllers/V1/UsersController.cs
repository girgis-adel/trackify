using Api.Controllers.V1.Infrastructure;
using Application.Features.Identity;
using Application.Features.Identity.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers.V1;

[Authorize]
public sealed class UsersController(IIdentityService identityService) : BaseV1Controller
{
    [HttpGet("me")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(id))
            return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "The token does not identify a user.");

        var result = await identityService.GetCurrentUserAsync(id, cancellationToken);
        return result.ToActionResult(this);
    }
}

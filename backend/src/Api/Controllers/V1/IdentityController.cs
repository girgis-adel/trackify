using Api.Controllers.V1.Infrastructure;
using Application.Features.Identity;
using Application.Features.Identity.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.V1;

public sealed class IdentityController(IIdentityService identityService) : BaseV1Controller
{
    [AllowAnonymous]
    [HttpPost("login")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await identityService.LoginAsync(request, cancellationToken);
        if (result.IsFailed)
            return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "Unable to sign in with these credentials.");

        return Ok(result.Value);
    }
}

using Api.Controllers.V1.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.V1;

public sealed class IdentityController : BaseV1Controller
{
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] object request, CancellationToken cancellationToken)
    {
        return Ok();
    }
}

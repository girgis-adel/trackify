using Api.Controllers.V1.Infrastructure;
using Application.Features.UI;
using Application.Features.UI.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.V1;

[AllowAnonymous]
public sealed class UIController(IUIService uiService) : BaseV1Controller
{
    [HttpGet("bootstrap")]
    [ProducesResponseType<BootstrapResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBootstrapAsync(CancellationToken cancellationToken)
    {
        return Ok(await uiService.GetBootstrapAsync(cancellationToken));
    }
}

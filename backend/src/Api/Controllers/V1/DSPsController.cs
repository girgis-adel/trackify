using Api.Controllers.V1.Infrastructure;
using Application.Features.DSPs;
using Application.Features.DSPs.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.V1;

[AllowAnonymous]
public sealed class DSPsController(IDSPService dspService) : BaseV1Controller
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<DSPResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        return Ok(await dspService.GetAllAsync(cancellationToken));
    }
}

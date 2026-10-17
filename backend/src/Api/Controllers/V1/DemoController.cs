using Api.Controllers.V1.Infrastructure;
using Application.Features.Demo;
using Application.Features.Demo.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.V1;

[AllowAnonymous]
public sealed class DemoController(IDemoService demoService) : BaseV1Controller
{
    [HttpGet("accounts")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType<IReadOnlyList<DemoAccountResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccountsAsync(CancellationToken cancellationToken)
    {
        var result = await demoService.GetAccountsAsync(cancellationToken);
        return result.ToActionResult(this);
    }
}

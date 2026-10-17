using Api.Controllers.V1.Infrastructure;
using Application.Features.Distributions;
using Application.Features.Distributions.Dtos;
using Application.Features.Tracks;
using Application.Features.Tracks.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.V1;

public sealed class TracksController(ITrackService trackService, IDistributionService distributionService) : BaseV1Controller
{
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TrackResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync([FromQuery] TrackFilterRequest request, CancellationToken cancellationToken)
    {
        return Ok(await trackService.GetAllAsync(request, cancellationToken));
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType<TrackDetailsResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var result = await trackService.GetByIdAsync(id, cancellationToken);
        return result.ToActionResult(this);
    }

    [Authorize(Roles = CatalogRoles.Managers)]
    [HttpPost("{id}/distribute")]
    [ProducesResponseType<IReadOnlyList<DistributionResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> DistributeAsync(string id, [FromBody] DistributeTrackRequest request, CancellationToken cancellationToken)
    {
        var result = await distributionService.DistributeAsync(id, request, cancellationToken);
        return result.ToActionResult(this);
    }

    [Authorize(Roles = CatalogRoles.Managers)]
    [HttpPatch("{id}/status")]
    [ProducesResponseType<TrackResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateStatusAsync(string id, [FromBody] UpdateTrackStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await trackService.UpdateStatusAsync(id, request, cancellationToken);
        return result.ToActionResult(this);
    }
}

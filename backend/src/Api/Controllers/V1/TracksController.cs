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
    [Authorize(Roles = CatalogRoles.Managers)]
    [HttpPost]
    [ProducesResponseType<TrackResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateTrackRequest request, CancellationToken cancellationToken)
    {
        var result = await trackService.CreateAsync(request, cancellationToken);
        if (result.IsFailed)
            return result.ToActionResult(this);

        return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Value.Id, version = "1.0" }, result.Value);
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TrackResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync([FromQuery] TrackFilterRequest request, CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();
        if (Request.Query.ContainsKey("artistId") && string.IsNullOrWhiteSpace(request.ArtistId))
            errors[nameof(request.ArtistId)] = ["Artist ID cannot be empty."];
        if (Request.Query.ContainsKey("genre") && string.IsNullOrWhiteSpace(request.Genre))
            errors[nameof(request.Genre)] = ["Genre cannot be empty."];
        if (Request.Query.ContainsKey("status") && !request.Status.HasValue)
            errors[nameof(request.Status)] = ["Status cannot be empty."];
        if (errors.Count > 0)
            return ValidationProblem(new ValidationProblemDetails(errors));

        return Ok(await trackService.GetAllAsync(request, cancellationToken));
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    [ActionName(nameof(GetByIdAsync))]
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

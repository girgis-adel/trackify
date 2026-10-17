using Api.Controllers.V1.Infrastructure;
using Application.Features.Artists;
using Application.Features.Artists.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.V1;

public sealed class ArtistsController(IArtistService artistService) : BaseV1Controller
{
    [Authorize(Roles = CatalogRoles.Managers)]
    [HttpPost]
    [ProducesResponseType<ArtistResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateArtistRequest request, CancellationToken cancellationToken)
    {
        var result = await artistService.CreateAsync(request, cancellationToken);
        if (result.IsFailed)
            return result.ToActionResult(this);

        return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Value.Id, version = "1.0" }, result.Value);
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ArtistResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        return Ok(await artistService.GetAllAsync(cancellationToken));
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    [ActionName(nameof(GetByIdAsync))]
    [ProducesResponseType<ArtistResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var result = await artistService.GetByIdAsync(id, cancellationToken);
        return result.ToActionResult(this);
    }
}

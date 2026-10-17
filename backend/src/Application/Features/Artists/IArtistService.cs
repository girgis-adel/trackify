using Application.Features.Artists.Dtos;
using FluentResults;

namespace Application.Features.Artists;

public interface IArtistService
{
    Task<Result<ArtistResponse>> CreateAsync(CreateArtistRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ArtistResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<ArtistResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default);
}

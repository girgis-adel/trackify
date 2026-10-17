using Application.Features.Tracks.Dtos;
using FluentResults;

namespace Application.Features.Tracks;

public interface ITrackService
{
    Task<IReadOnlyList<TrackResponse>> GetAllAsync(TrackFilterRequest request, CancellationToken cancellationToken = default);

    Task<Result<TrackDetailsResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<Result<TrackResponse>> UpdateStatusAsync(string id, UpdateTrackStatusRequest request, CancellationToken cancellationToken = default);
}

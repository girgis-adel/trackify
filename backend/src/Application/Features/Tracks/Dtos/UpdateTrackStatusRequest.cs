using Domain.Enums;

namespace Application.Features.Tracks.Dtos;

/// <summary>Changes a track to one of the supported domain statuses.</summary>
public sealed record UpdateTrackStatusRequest
{
    public TrackStatus Status { get; init; }
}

using Domain.Enums;

namespace Application.Features.Tracks.Dtos;

/// <summary>Optional filters for browsing the track catalog.</summary>
public sealed record TrackFilterRequest
{
    public string? ArtistId { get; init; }

    public string? Genre { get; init; }

    public TrackStatus? Status { get; init; }
}

namespace Application.Features.Tracks.Dtos;

/// <summary>Creates a draft track owned by an existing artist.</summary>
public sealed record CreateTrackRequest
{
    public string ArtistId { get; init; } = string.Empty;

    public string Isrc { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string Genre { get; init; } = string.Empty;

    public DateTime? ReleaseDate { get; init; }
}

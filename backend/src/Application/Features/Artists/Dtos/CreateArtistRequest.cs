namespace Application.Features.Artists.Dtos;

/// <summary>Creates an artist profile for an existing Identity user.</summary>
public sealed record CreateArtistRequest
{
    public string UserId { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? Country { get; init; }
}

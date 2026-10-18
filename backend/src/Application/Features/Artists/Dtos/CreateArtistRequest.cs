namespace Application.Features.Artists.Dtos;

/// <summary>Creates an artist account and its linked profile.</summary>
public sealed record CreateArtistRequest
{
    public string UserName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? Country { get; init; }
}

namespace Application.Features.Artists.Dtos;

/// <summary>Public artist profile information.</summary>
public sealed record ArtistResponse(string Id, string Name, string Email, string? Country);

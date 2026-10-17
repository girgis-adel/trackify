namespace Application.Features.Identity.Dtos;

/// <summary>Authenticated user context and capabilities supported by the current API.</summary>
public sealed record UserResponse(
    string Id,
    string DisplayName,
    string Email,
    IReadOnlyList<string> Roles,
    string? ArtistId,
    bool CanManageCatalog);

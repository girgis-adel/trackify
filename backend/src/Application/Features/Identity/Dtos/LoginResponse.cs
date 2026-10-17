namespace Application.Features.Identity.Dtos;

/// <summary>A JWT issued by the configured authentication infrastructure.</summary>
public sealed record LoginResponse(string AccessToken, string TokenType, DateTimeOffset ExpiresAt, UserResponse User);

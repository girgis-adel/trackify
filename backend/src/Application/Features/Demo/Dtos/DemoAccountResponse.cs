namespace Application.Features.Demo.Dtos;

/// <summary>Intentionally public credentials for an explicitly enabled isolated demonstration.</summary>
public sealed record DemoAccountResponse(string Persona, string DisplayName, string Email, string Password, string Description);

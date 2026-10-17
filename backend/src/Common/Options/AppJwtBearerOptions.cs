using System.ComponentModel.DataAnnotations;

namespace Common.Options;

public sealed class AppJwtBearerOptions
{
    [Required]
    public string SecretKey { get; set; } = string.Empty;

    [Required]
    public double ExpirationInMinutes { get; set; }

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;

namespace Common.Options;

public sealed class AppJwtBearerOptions
{
    [Required]
    [MinLength(32)]
    public string SecretKey { get; set; } = string.Empty;

    [Range(1, 1440)]
    public double ExpirationInMinutes { get; set; }

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;
}

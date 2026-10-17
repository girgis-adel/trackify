using System.ComponentModel.DataAnnotations;

namespace Common.Options;

public sealed class SuperAdminUser
{
    [Required]
    public string UserName { get; set; } = "girgisdev";

    [Required]
    public string Password { get; set; } = "Pass@123";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "girgisdev@gmail.com";

    public string? PhoneNumber { get; set; } = "+201129310299";
}

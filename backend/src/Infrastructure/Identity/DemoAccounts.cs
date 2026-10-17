using Domain.Enums;

namespace Infrastructure.Identity;

public static class DemoAccounts
{
    public const string Password = "Demo@12345";

    public static IReadOnlyList<DemoAccount> Accounts { get; } = Array.AsReadOnly<DemoAccount>(
    [
        new("trackify-demo-superadmin", "demo.superadmin", "superadmin@demo.local", "Super Administrator", Roles.SuperAdmin, "Manage artists, track statuses and distribution."),
        new("trackify-demo-admin", "demo.admin", "admin@demo.local", "Administrator", Roles.Admin, "Manage the music catalog."),
        new("trackify-demo-artist", "demo.artist", "artist@demo.local", "Demo Artist", Roles.Artist, "Browse the catalog and view the linked artist profile."),
        new("trackify-demo-visitor", "demo.visitor", "visitor@demo.local", "Visitor", Roles.Visitor, "Explore the catalog with an authenticated viewer account."),
    ]);

    public sealed record DemoAccount(string Id, string UserName, string Email, string DisplayName, Roles Role, string Description);
}

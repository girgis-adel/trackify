using Bogus;
using Common.Options;
using Domain.Entities.Artists;
using Domain.Entities.Identity;
using Domain.Entities.Tracks;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Infrastructure.Persistence.Contexts;

public sealed class SqlServerDatabaseContextInitializer( 
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    SqlServerDatabaseContext databaseContext,
    ILookupNormalizer lookupNormalizer,
    IOptions<SuperAdminUser>? SuperAdminUserAccessor = null)
{
    private readonly SuperAdminUser superAdminUser = SuperAdminUserAccessor?.Value ?? new SuperAdminUser();

    public async Task InitializeAsync()
    {
        await SeedRolesAsync();
        await SeedSuperAdminUserAsync();
        await SeedAdminUsersAsync();
        await SeedArtistUsersAsync();
    }

    private async Task SeedRolesAsync()
    {
        var roleNames = Enum.GetNames<Roles>();

        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new Role { Name = roleName });
            }
        }
    }

    private async Task SeedSuperAdminUserAsync()
    {
        var superAdminEmail = superAdminUser.Email;
        var superAdminPassword = superAdminUser.Password;
        if (string.IsNullOrWhiteSpace(superAdminEmail) || string.IsNullOrWhiteSpace(superAdminPassword))
        {
            throw new InvalidOperationException("Super admin user credentials are not configured.");
        }
        var existingUser = await userManager.FindByEmailAsync(superAdminEmail);
        if (existingUser == null)
        {
            var superAdmin = new User
            {
                UserName = superAdminUser.UserName,
                Email = superAdminEmail,
                PhoneNumber = superAdminUser.PhoneNumber,
                PhoneNumberConfirmed = true,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(superAdmin, superAdminPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create super admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            await userManager.AddToRoleAsync(superAdmin, Roles.SuperAdmin.ToString());
        }
    }

    private async Task SeedAdminUsersAsync()
    {
        var admins = await userManager.GetUsersInRoleAsync(Roles.Admin.ToString());

        if (!admins.Any())
        {
            var fakeAdminsFaker = new Faker<FakeUser>()
                .StrictMode(true)
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.UserName, (f, u) => f.Internet.UserName(u.FirstName, u.LastName))
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
                .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber())
                .RuleFor(u => u.Password, f => f.Internet.Password(2, prefix: "Pass@123"));

            var fakeAdmins = fakeAdminsFaker.Generate(5);

            foreach (var fakeAdmin in fakeAdmins)
            {
                var admin = new User
                {
                    UserName = fakeAdmin.UserName,
                    Email = fakeAdmin.Email,
                    PhoneNumber = fakeAdmin.PhoneNumber,
                    PhoneNumberConfirmed = true,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(admin, fakeAdmin.Password!);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
                await userManager.AddToRoleAsync(admin, Roles.Admin.ToString());

                // for development purposes, we can add a claim to store the password in plain text (not recommended for production)
                await userManager.AddClaimAsync(admin, new Claim("password", fakeAdmin.Password!));
            }
        }
    }

    private async Task SeedArtistUsersAsync()
    {
        var artists = await userManager.GetUsersInRoleAsync(Roles.Artist.ToString());

        if (!artists.Any())
        {
            var fakeArtistsFaker = new Faker<FakeUser>()
                .StrictMode(true)
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.UserName, (f, u) => f.Internet.UserName(u.FirstName, u.LastName))
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
                .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber())
                .RuleFor(u => u.Password, f => f.Internet.Password(2, prefix: "Pass@123"));
            var fakeArtists = fakeArtistsFaker.Generate(5);
            foreach (var fakeArtist in fakeArtists)
            {
                var artist = new User
                {
                    UserName = fakeArtist.UserName,
                    Email = fakeArtist.Email,
                    PhoneNumber = fakeArtist.PhoneNumber,
                    PhoneNumberConfirmed = true,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(artist, fakeArtist.Password!);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create artist user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
                await userManager.AddToRoleAsync(artist, Roles.Artist.ToString());
                // for development purposes, we can add a claim to store the password in plain text (not recommended for production)
                await userManager.AddClaimAsync(artist, new Claim("password", fakeArtist.Password!));
            }
        }
    }

    private async Task SeedTracksAsync()
    {
        if (!databaseContext.Tracks.Any())
        {
            var fakeTracksFaker = new Faker<FakeTrack>()
                .StrictMode(true)
                .RuleFor(t => t.Isrc, f => f.Random.AlphaNumeric(12).ToUpper())
                .RuleFor(t => t.Title, f => f.Lorem.Sentence(4))
                .RuleFor(t => t.ReleaseDate, f => f.Date.Past())
                .RuleFor(t => t.Genre, f => f.Music.Genre())
                .RuleFor(t => t.Status, f => f.Random.Enum<TrackStatus>())
                .RuleFor(t => t.IsActive, f => f.Random.Bool())
                .RuleFor(t => t.IsRemovable, f => f.Random.Bool())
                .RuleFor(t => t.RemovedAt, (f, t) => t.IsRemovable ? f.Date.PastOffset() : null);

            var fakeTracks = fakeTracksFaker.Generate(20);

            foreach (var item in fakeTracks)
            {
                await databaseContext.Tracks.AddAsync(new Track
                {
                    Isrc = item.Isrc!,
                    Title = item.Title!,
                    ReleaseDate = item.ReleaseDate!.Value,
                    Genre = item.Genre!,
                    Status = item.Status!.Value,
                    IsActive = item.IsActive,
                    IsRemovable = item.IsRemovable,
                    RemovedAt = item.RemovedAt,
                });
            }

            await databaseContext.SaveChangesAsync();
        }
    }

    private sealed class FakeUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Password { get; set; }
    }

    private sealed class FakeTrack
    {
        public string? Isrc { get; set; }
        public string? Title { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string? Genre { get; set; }
        public TrackStatus? Status { get; set; }
        public bool IsActive { get; set; }
        public bool IsRemovable { get; set; }
        public DateTimeOffset? RemovedAt { get; set; }
        public string? Country { get; set; }
    }
}

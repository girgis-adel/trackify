using Bogus;
using Common.Options;
using Domain.Entities.Artists;
using Domain.Entities.DSPs;
using Domain.Entities.Identity;
using Domain.Entities.TrackDistributions;
using Domain.Entities.Tracks;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

    private static readonly ArtistSeed[] ArtistSeeds =
    [
        new("Mara Voss", "mara.voss", "mara.voss@example.test", "United States", "Pass@123Mara"),
        new("Nia Okafor", "nia.okafor", "nia.okafor@example.test", "Nigeria", "Pass@123Nia"),
        new("Lucía Reyes", "lucia.reyes", "lucia.reyes@example.test", "Mexico", "Pass@123Lucia")
    ];

    private static readonly TrackSeed[] TrackSeeds =
    [
        new("USTFY2600001", "Copper Skies", "Indie Folk", TrackStatus.Draft, null),
        new("USTFY2600002", "Meridian Drive", "Synthpop", TrackStatus.Submitted, new DateTime(2026, 11, 20)),
        new("USTFY2400003", "Tidal Patterns", "Ambient", TrackStatus.Distributed, new DateTime(2024, 6, 14)),
        new("USTFY2500004", "Green Line", "Jazz", TrackStatus.Distributed, new DateTime(2025, 3, 21)),
        new("USTFY2600005", "After the Rain", "R&B", TrackStatus.Submitted, new DateTime(2026, 12, 4)),
        new("USTFY2500006", "Lanterns at Dawn", "Electronic", TrackStatus.Distributed, new DateTime(2025, 8, 8)),
        new("USTFY2600007", "Northbound Lanes", "Alternative Rock", TrackStatus.Draft, null),
        new("USTFY2600008", "Solstice Echoes", "Hip-Hop", TrackStatus.Distributed, new DateTime(2026, 5, 29))
    ];

    private static readonly DistributionSeed[] DistributionSeeds =
    [
        new("USTFY2400003", "Spotify", TrackDistributionStatus.Live),
        new("USTFY2400003", "Apple Music", TrackDistributionStatus.Live),
        new("USTFY2400003", "YouTube Music", TrackDistributionStatus.Live),
        new("USTFY2500004", "Spotify", TrackDistributionStatus.Live),
        new("USTFY2500004", "Apple Music", TrackDistributionStatus.Live),
        new("USTFY2500006", "Spotify", TrackDistributionStatus.Live),
        new("USTFY2500006", "YouTube Music", TrackDistributionStatus.Live),
        new("USTFY2600008", "Apple Music", TrackDistributionStatus.Live),
        new("USTFY2600002", "Spotify", TrackDistributionStatus.Pending),
        new("USTFY2600002", "Apple Music", TrackDistributionStatus.Pending),
        new("USTFY2600005", "Spotify", TrackDistributionStatus.Rejected),
        new("USTFY2600005", "YouTube Music", TrackDistributionStatus.Pending)
    ];

    public async Task InitializeAsync()
    {
        await SeedRolesAsync();
        await SeedSuperAdminUserAsync();
        await SeedAdminUsersAsync();
        await SeedArtistUsersAsync();
        await SeedDSPsAsync();
        await SeedTracksAsync();
        await SeedTrackDistributionsAsync();
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
        var normalizedEmail = lookupNormalizer.NormalizeEmail(superAdminEmail);
        var superAdmin = await databaseContext.Users.IgnoreQueryFilters()
            .SingleOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);
        if (superAdmin is null)
        {
            superAdmin = new User
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
        }

        if (!superAdmin.IsActive || superAdmin.RemovedAt is not null)
        {
            throw new InvalidOperationException("The configured super admin user is inactive or removed.");
        }

        if (!await userManager.IsInRoleAsync(superAdmin, Roles.SuperAdmin.ToString()))
        {
            var result = await userManager.AddToRoleAsync(superAdmin, Roles.SuperAdmin.ToString());
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to assign super admin role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }

    private async Task SeedAdminUsersAsync()
    {
        var admins = await userManager.GetUsersInRoleAsync(Roles.Admin.ToString());

        if (!admins.Any())
        {
            var fakeAdminsFaker = new Faker<UserSeed>()
                .UseSeed(20261018)
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
                var normalizedEmail = lookupNormalizer.NormalizeEmail(fakeAdmin.Email!);
                var admin = await databaseContext.Users.IgnoreQueryFilters()
                    .SingleOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);
                var created = admin is null;
                if (admin is null)
                {
                    admin = new User
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
                }
                else if (admin.NormalizedUserName != lookupNormalizer.NormalizeName(fakeAdmin.UserName!))
                {
                    throw new InvalidOperationException($"The seed admin email {fakeAdmin.Email} belongs to another user.");
                }

                if (!admin.IsActive || admin.RemovedAt is not null)
                {
                    throw new InvalidOperationException($"The seed admin user {fakeAdmin.Email} is inactive or removed.");
                }

                if (!await userManager.IsInRoleAsync(admin, Roles.Admin.ToString()))
                {
                    var result = await userManager.AddToRoleAsync(admin, Roles.Admin.ToString());
                    if (!result.Succeeded)
                    {
                        throw new InvalidOperationException($"Failed to assign admin role to {fakeAdmin.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }

                if (created)
                {
                    // for development purposes, we can add a claim to store the password in plain text (not recommended for production)
                    await userManager.AddClaimAsync(admin, new Claim("password", fakeAdmin.Password!));
                }
            }
        }
    }

    private async Task SeedArtistUsersAsync()
    {
        foreach (var seed in ArtistSeeds)
        {
            var normalizedEmail = lookupNormalizer.NormalizeEmail(seed.Email);
            var user = await databaseContext.Users.IgnoreQueryFilters()
                .SingleOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);

            if (user is null)
            {
                user = new User
                {
                    UserName = seed.UserName,
                    Email = seed.Email,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, seed.Password);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create artist user {seed.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else if (user.NormalizedUserName != lookupNormalizer.NormalizeName(seed.UserName))
            {
                throw new InvalidOperationException($"The seed artist email {seed.Email} belongs to another user.");
            }

            if (!user.IsActive || user.RemovedAt is not null)
            {
                throw new InvalidOperationException($"The seed artist user {seed.Email} is inactive or removed.");
            }

            if (!await userManager.IsInRoleAsync(user, Roles.Artist.ToString()))
            {
                var result = await userManager.AddToRoleAsync(user, Roles.Artist.ToString());
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to assign artist role to {seed.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            if (!await databaseContext.Artists.IgnoreQueryFilters().AnyAsync(a => a.UserId == user.Id))
            {
                databaseContext.Artists.Add(new Artist
                {
                    UserId = user.Id,
                    Name = seed.Name,
                    EmailSnapshot = user.Email!,
                    Country = seed.Country,
                });
                await databaseContext.SaveChangesAsync();
            }
        }
    }

    private async Task SeedDSPsAsync()
    {
        foreach (var name in new[] { "Spotify", "Apple Music", "YouTube Music" })
        {
            var normalizedName = lookupNormalizer.NormalizeName(name);
            if (!await databaseContext.DSPs.AnyAsync(d => d.NormalizedName == normalizedName))
            {
                databaseContext.DSPs.Add(new DSP { Name = name, NormalizedName = normalizedName });
            }
        }

        await databaseContext.SaveChangesAsync();
    }

    private async Task SeedTracksAsync()
    {
        var isrcs = TrackSeeds.Select(t => t.Isrc).ToArray();
        var existingIsrcs = await databaseContext.Tracks.IgnoreQueryFilters()
            .Where(t => isrcs.Contains(t.Isrc))
            .Select(t => t.Isrc)
            .ToListAsync();
        var existing = new HashSet<string>(existingIsrcs, StringComparer.OrdinalIgnoreCase);

        foreach (var seed in TrackSeeds)
        {
            if (existing.Add(seed.Isrc))
            {
                databaseContext.Tracks.Add(new Track
                {
                    Isrc = seed.Isrc,
                    Title = seed.Title,
                    Genre = seed.Genre,
                    Status = seed.Status,
                    ReleaseDate = seed.ReleaseDate,
                });
            }
        }

        await databaseContext.SaveChangesAsync();
    }

    private async Task SeedTrackDistributionsAsync()
    {
        var isrcs = TrackSeeds.Select(t => t.Isrc).ToArray();
        var matchingTracks = await databaseContext.Tracks.IgnoreQueryFilters()
            .Where(t => isrcs.Contains(t.Isrc))
            .ToListAsync();
        var tracks = matchingTracks.ToDictionary(t => t.Isrc, StringComparer.OrdinalIgnoreCase);
        var normalizedDspNames = DistributionSeeds.Select(d => lookupNormalizer.NormalizeName(d.DspName)).Distinct().ToArray();
        var matchingDsps = await databaseContext.DSPs
            .Where(d => normalizedDspNames.Contains(d.NormalizedName))
            .ToListAsync();
        var dsps = matchingDsps.ToDictionary(d => d.NormalizedName, StringComparer.OrdinalIgnoreCase);
        var trackIds = tracks.Values.Select(t => t.Id).ToArray();
        var dspIds = dsps.Values.Select(d => d.Id).ToArray();
        var existing = await databaseContext.TrackDistributions
            .Where(d => trackIds.Contains(d.TrackId) && dspIds.Contains(d.DSPId))
            .Select(d => new { d.TrackId, d.DSPId })
            .ToListAsync();
        var existingPairs = existing.Select(d => (d.TrackId, d.DSPId)).ToHashSet();

        foreach (var seed in DistributionSeeds)
        {
            var track = tracks[seed.Isrc];
            var dsp = dsps[lookupNormalizer.NormalizeName(seed.DspName)];

            // Preserve changes to existing tracks instead of adding contradictory distributions.
            if (!track.IsActive || track.RemovedAt is not null ||
                (seed.Status == TrackDistributionStatus.Live && (track.Status != TrackStatus.Distributed || track.ReleaseDate is null)) ||
                (seed.Status != TrackDistributionStatus.Live && track.Status != TrackStatus.Submitted))
            {
                continue;
            }

            if (existingPairs.Add((track.Id, dsp.Id)))
            {
                databaseContext.TrackDistributions.Add(new TrackDistribution
                {
                    TrackId = track.Id,
                    DSPId = dsp.Id,
                    Status = seed.Status,
                    GoLiveDate = seed.Status == TrackDistributionStatus.Live ? track.ReleaseDate : null,
                });
            }
        }

        await databaseContext.SaveChangesAsync();
    }

    private sealed class UserSeed
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Password { get; set; }
    };

    private sealed record ArtistSeed(string Name, string UserName, string Email, string Country, string Password);

    private sealed record TrackSeed(string Isrc, string Title, string Genre, TrackStatus Status, DateTime? ReleaseDate);

    private sealed record DistributionSeed(string Isrc, string DspName, TrackDistributionStatus Status);
}

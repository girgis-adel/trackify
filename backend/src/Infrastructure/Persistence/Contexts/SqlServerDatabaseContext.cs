using Domain.Entities.Artists;
using Domain.Entities.DSPs;
using Domain.Entities.Identity;
using Domain.Entities.TrackDistributions;
using Domain.Entities.Tracks;
using Infrastructure.Persistence.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Contexts;

public sealed class SqlServerDatabaseContext : IdentityDbContext<User, Role, string>
{
    public DbSet<Artist> Artists { get; set; }

    public DbSet<DSP> DSPs { get; set; }

    public DbSet<TrackDistribution> TrackDistributions { get; set; }

    public DbSet<Track> Tracks { get; set; }

    public SqlServerDatabaseContext() : base() { }

    public SqlServerDatabaseContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly);
        builder.ApplySoftRemovableQueryFilters();
        builder.ApplyIsActiveQueryFilters();
        builder.ConfigureVersion();
        builder.ApplyCreateDateProtectionBehavior();
    }
}

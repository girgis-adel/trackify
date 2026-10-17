using Domain.Entities.Tracks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class TrackConfiguration : IEntityTypeConfiguration<Track>
{
    public void Configure(EntityTypeBuilder<Track> builder)
    {
        builder.Property(t => t.Isrc).HasMaxLength(256);
        builder.HasIndex(t => t.Isrc).IsUnique();

        builder.Property(t => t.Title).HasMaxLength(256);

        builder.Property(t => t.Genre).HasMaxLength(128);

        builder.Property(t => t.Status).HasConversion<string>();
    }
}

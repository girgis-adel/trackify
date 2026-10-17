using Domain.Entities.Artists;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class ArtistConfiguration : IEntityTypeConfiguration<Artist>
{
    public void Configure(EntityTypeBuilder<Artist> builder)
    {
        builder.Property(a => a.Name).HasMaxLength(256);

        builder.Property(a => a.EmailSnapshot).HasMaxLength(256);
        builder.HasIndex(a => a.EmailSnapshot);

        builder.Property(a => a.Country).HasMaxLength(256);
    }
}

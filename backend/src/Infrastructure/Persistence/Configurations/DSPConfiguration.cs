using Domain.Entities.DSPs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class DSPConfiguration : IEntityTypeConfiguration<DSP>
{
    public void Configure(EntityTypeBuilder<DSP> builder)
    {
        builder.Property(d => d.Name).HasMaxLength(256);
        builder.Property(d => d.NormalizedName).HasMaxLength(256);
        builder.HasIndex(d => d.NormalizedName).IsUnique();
    }
}

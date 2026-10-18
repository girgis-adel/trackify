using Domain.Entities.TrackDistributions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace Infrastructure.Persistence.Configurations;

internal sealed class TrackDistributionConfiguration : IEntityTypeConfiguration<TrackDistribution>
{
    public void Configure(EntityTypeBuilder<TrackDistribution> builder)
    {
        builder.Property(td => td.Status).HasConversion<string>();
        builder.HasQueryFilter(d => !d.Track.IsRemovable || d.Track.RemovedAt == null);
    }
}

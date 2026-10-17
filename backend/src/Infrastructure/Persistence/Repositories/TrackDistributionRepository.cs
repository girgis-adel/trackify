using Domain.Entities.TrackDistributions;
using Infrastructure.Persistence.Contexts;

namespace Infrastructure.Persistence.Repositories;

public sealed class TrackDistributionRepository(SqlServerDatabaseContext databaseContext)
    : Repository<TrackDistribution>(databaseContext), ITrackDistributionRepository;

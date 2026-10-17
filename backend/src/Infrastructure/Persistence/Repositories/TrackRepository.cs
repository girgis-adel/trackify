using Domain.Entities.Tracks;
using Infrastructure.Persistence.Contexts;

namespace Infrastructure.Persistence.Repositories;

public sealed class TrackRepository(SqlServerDatabaseContext databaseContext)
    : Repository<Track>(databaseContext), ITrackRepository;

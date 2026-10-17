using Domain.Entities.Artists;
using Infrastructure.Persistence.Contexts;

namespace Infrastructure.Persistence.Repositories;

public sealed class ArtistRepository(SqlServerDatabaseContext databaseContext)
    : Repository<Artist>(databaseContext), IArtistRepository;

using Common.Errors;
using Domain.Entities.DSPs;
using FluentResults;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class DSPRepository(SqlServerDatabaseContext databaseContext)
    : Repository<DSP>(databaseContext), IDSPRepository
{
    public async Task<Result<DSP>> GetByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken = default)
    {
        var dsp = await Store.AsNoTracking()
            .SingleOrDefaultAsync(d => d.NormalizedName == normalizedName, cancellationToken);

        return dsp is null
            ? Result.Fail<DSP>(new NotFoundError("DSP was not found."))
            : Result.Ok(dsp);
    }

    public async Task<Result<DSP>> FindByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken = default)
    {
        var dsp = await Store
            .SingleOrDefaultAsync(d => d.NormalizedName == normalizedName, cancellationToken);

        return dsp is null
            ? Result.Fail<DSP>(new NotFoundError("DSP was not found."))
            : Result.Ok(dsp);
    }
}

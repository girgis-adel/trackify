using Common.Abstractions;
using FluentResults;

namespace Domain.Entities.DSPs;

public interface IDSPRepository : IRepository<DSP>
{
    Task<Result<DSP>> GetByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken = default);

    Task<Result<DSP>> FindByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken = default);
}

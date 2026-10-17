using Application.Features.Distributions.Dtos;
using FluentResults;

namespace Application.Features.Distributions;

public interface IDistributionService
{
    Task<Result<IReadOnlyList<DistributionResponse>>> DistributeAsync(string trackId, DistributeTrackRequest request, CancellationToken cancellationToken = default);
}

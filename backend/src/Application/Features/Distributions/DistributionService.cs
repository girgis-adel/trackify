using Application.Features.Distributions.Dtos;
using Common.Errors;
using Domain.Entities.DSPs;
using Domain.Entities.TrackDistributions;
using Domain.Entities.Tracks;
using Domain.Enums;
using FluentResults;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Distributions;

public sealed class DistributionService(
    ITrackRepository trackRepository,
    IDSPRepository dspRepository,
    ITrackDistributionRepository distributionRepository,
    IValidator<DistributeTrackRequest> validator,
    IValidator<string> idValidator) : IDistributionService
{
    public async Task<Result<IReadOnlyList<DistributionResponse>>> DistributeAsync(string trackId, DistributeTrackRequest request, CancellationToken cancellationToken = default)
    {
        await idValidator.ValidateAndThrowAsync(trackId, cancellationToken);
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var result = await trackRepository.FindByIdAsync(trackId, cancellationToken);
        if (result.IsFailed)
            return Result.Fail<IReadOnlyList<DistributionResponse>>(result.Errors);

        var dsps = await dspRepository.Store.AsNoTracking()
            .Where(d => request.DSPIds.Contains(d.Id))
            .OrderBy(d => d.Name).ThenBy(d => d.Id)
            .ToListAsync(cancellationToken);
        if (dsps.Count != request.DSPIds.Length)
            return Result.Fail<IReadOnlyList<DistributionResponse>>(new NotFoundError("One or more DSPs were not found."));

        if (await distributionRepository.Store.AnyAsync(d => d.TrackId == trackId && request.DSPIds.Contains(d.DSPId), cancellationToken))
            return Result.Fail<IReadOnlyList<DistributionResponse>>(new ConflictError("The track already has a distribution for one or more selected DSPs."));

        var distributions = new List<TrackDistribution>();
        foreach (var dsp in dsps)
        {
            var distribution = new TrackDistribution
            {
                TrackId = result.Value.Id,
                DSPId = dsp.Id,
                Status = TrackDistributionStatus.Pending,
            };
            var createResult = await distributionRepository.CreateAsync(distribution, cancellationToken);
            if (createResult.IsFailed)
                return Result.Fail<IReadOnlyList<DistributionResponse>>(createResult.Errors);

            distributions.Add(distribution);
        }

        if (result.Value.Status == TrackStatus.Draft)
            result.Value.Status = TrackStatus.Submitted;

        // Both repositories share the scoped context; commit the entire submission once.
        await distributionRepository.SaveChangesAsync(cancellationToken);

        var names = dsps.ToDictionary(d => d.Id, d => d.Name);
        IReadOnlyList<DistributionResponse> response = distributions
            .Select(d => new DistributionResponse(d.Id, d.DSPId, names[d.DSPId], d.CreateDate, d.Status, d.GoLiveDate))
            .ToList();

        return Result.Ok(response);
    }
}

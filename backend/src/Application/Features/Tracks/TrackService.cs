using Application.Features.Distributions.Dtos;
using Application.Features.Tracks.Dtos;
using Domain.Entities.TrackDistributions;
using Domain.Entities.Tracks;
using FluentResults;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Tracks;

public sealed class TrackService(
    ITrackRepository trackRepository,
    ITrackDistributionRepository distributionRepository,
    IValidator<TrackFilterRequest> filterValidator,
    IValidator<UpdateTrackStatusRequest> statusValidator,
    IValidator<string> idValidator) : ITrackService
{
    public async Task<IReadOnlyList<TrackResponse>> GetAllAsync(TrackFilterRequest request, CancellationToken cancellationToken = default)
    {
        await filterValidator.ValidateAndThrowAsync(request, cancellationToken);

        var tracks = trackRepository.Store;
        if (request.Genre is not null)
        {
            var genre = request.Genre.Trim();
            tracks = tracks.Where(t => t.Genre == genre);
        }

        if (request.Status.HasValue)
            tracks = tracks.Where(t => t.Status == request.Status.Value);

        return await tracks.OrderBy(t => t.Title).ThenBy(t => t.Id)
            .Select(t => new TrackResponse(t.Id, t.Isrc, t.Title, t.ReleaseDate, t.Genre, t.Status))
            .ToListAsync(cancellationToken);
    }

    public async Task<Result<TrackDetailsResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        await idValidator.ValidateAndThrowAsync(id, cancellationToken);

        var result = await trackRepository.GetByIdAsync(id, cancellationToken);
        if (result.IsFailed)
            return Result.Fail<TrackDetailsResponse>(result.Errors);

        var distributions = await distributionRepository.Store
            .Where(d => d.TrackId == id)
            .OrderBy(d => d.DSP.Name).ThenBy(d => d.Id)
            .Select(d => new DistributionResponse(d.Id, d.DSPId, d.DSP.Name, d.CreateDate, d.Status, d.GoLiveDate))
            .ToListAsync(cancellationToken);

        var track = result.Value;
        return Result.Ok(new TrackDetailsResponse(track.Id, track.Isrc, track.Title, track.ReleaseDate, track.Genre, track.Status, distributions));
    }

    public async Task<Result<TrackResponse>> UpdateStatusAsync(string id, UpdateTrackStatusRequest request, CancellationToken cancellationToken = default)
    {
        await idValidator.ValidateAndThrowAsync(id, cancellationToken);
        await statusValidator.ValidateAndThrowAsync(request, cancellationToken);

        var result = await trackRepository.FindByIdAsync(id, cancellationToken);
        if (result.IsFailed)
            return Result.Fail<TrackResponse>(result.Errors);

        var track = result.Value;
        track.Status = request.Status;
        await trackRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok(new TrackResponse(track.Id, track.Isrc, track.Title, track.ReleaseDate, track.Genre, track.Status));
    }
}

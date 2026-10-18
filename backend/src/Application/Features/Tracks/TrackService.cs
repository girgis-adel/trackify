using Application.Features.Distributions.Dtos;
using Application.Features.Tracks.Dtos;
using Common.Errors;
using Domain.Entities.Artists;
using Domain.Entities.TrackDistributions;
using Domain.Entities.Tracks;
using Domain.Enums;
using FluentResults;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Tracks;

public sealed class TrackService(
    ITrackRepository trackRepository,
    IArtistRepository artistRepository,
    ITrackDistributionRepository distributionRepository,
    IValidator<CreateTrackRequest> createValidator,
    IValidator<TrackFilterRequest> filterValidator,
    IValidator<UpdateTrackStatusRequest> statusValidator,
    IValidator<string> idValidator) : ITrackService
{
    public async Task<Result<TrackResponse>> CreateAsync(CreateTrackRequest request, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var artistId = request.ArtistId.Trim();
        var artistName = await artistRepository.Store.AsNoTracking()
            .Where(a => a.Id == artistId).Select(a => a.Name)
            .SingleOrDefaultAsync(cancellationToken);
        if (artistName is null)
            return Result.Fail<TrackResponse>(new NotFoundError("Artist was not found."));

        var isrc = request.Isrc.Trim().ToUpperInvariant();
        if (await trackRepository.Store.IgnoreQueryFilters().AnyAsync(t => t.Isrc == isrc, cancellationToken))
            return Result.Fail<TrackResponse>(new ConflictError("A track with this ISRC already exists."));

        var track = new Track
        {
            ArtistId = artistId,
            Isrc = isrc,
            Title = request.Title.Trim(),
            Genre = request.Genre.Trim(),
            ReleaseDate = request.ReleaseDate,
            Status = TrackStatus.Draft,
        };
        var result = await trackRepository.CreateAsync(track, cancellationToken);
        if (result.IsFailed)
            return Result.Fail<TrackResponse>(result.Errors);

        await trackRepository.SaveChangesAsync(cancellationToken);
        return Result.Ok(new TrackResponse(track.Id, track.Isrc, track.Title, track.ReleaseDate, track.Genre, track.Status, artistId, artistName));
    }

    public async Task<IReadOnlyList<TrackResponse>> GetAllAsync(TrackFilterRequest request, CancellationToken cancellationToken = default)
    {
        await filterValidator.ValidateAndThrowAsync(request, cancellationToken);

        var tracks = trackRepository.Store.AsNoTracking();
        if (request.ArtistId is not null)
        {
            var artistId = request.ArtistId.Trim();
            tracks = tracks.Where(t => t.ArtistId == artistId);
        }
        if (request.Genre is not null)
        {
            var genre = request.Genre.Trim();
            tracks = tracks.Where(t => t.Genre == genre);
        }

        if (request.Status.HasValue)
            tracks = tracks.Where(t => t.Status == request.Status.Value);

        return await tracks.OrderBy(t => t.Title).ThenBy(t => t.Id)
            .Select(t => new TrackResponse(t.Id, t.Isrc, t.Title, t.ReleaseDate, t.Genre, t.Status, t.ArtistId, t.Artist.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<Result<TrackDetailsResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        await idValidator.ValidateAndThrowAsync(id, cancellationToken);

        var track = await trackRepository.Store.AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new { t.Id, t.Isrc, t.Title, t.ReleaseDate, t.Genre, t.Status, t.ArtistId, ArtistName = t.Artist.Name })
            .SingleOrDefaultAsync(cancellationToken);
        if (track is null)
            return Result.Fail<TrackDetailsResponse>(new NotFoundError("Track was not found."));

        var distributions = await distributionRepository.Store
            .Where(d => d.TrackId == id)
            .OrderBy(d => d.DSP.Name).ThenBy(d => d.Id)
            .Select(d => new DistributionResponse(d.Id, d.DSPId, d.DSP.Name, d.CreateDate, d.Status, d.GoLiveDate))
            .ToListAsync(cancellationToken);

        return Result.Ok(new TrackDetailsResponse(track.Id, track.Isrc, track.Title, track.ReleaseDate, track.Genre, track.Status, track.ArtistId, track.ArtistName, distributions));
    }

    public async Task<Result<TrackResponse>> UpdateStatusAsync(string id, UpdateTrackStatusRequest request, CancellationToken cancellationToken = default)
    {
        await idValidator.ValidateAndThrowAsync(id, cancellationToken);
        await statusValidator.ValidateAndThrowAsync(request, cancellationToken);

        var track = await trackRepository.Store.Include(t => t.Artist)
            .SingleOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (track is null)
            return Result.Fail<TrackResponse>(new NotFoundError("Track was not found."));
        track.Status = request.Status;
        await trackRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok(new TrackResponse(track.Id, track.Isrc, track.Title, track.ReleaseDate, track.Genre, track.Status, track.ArtistId, track.Artist.Name));
    }
}

using Application.Features.Artists.Dtos;
using Common.Errors;
using Domain.Entities.Artists;
using Domain.Entities.Identity;
using FluentResults;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Artists;

public sealed class ArtistService(
    IArtistRepository artistRepository,
    UserManager<User> userManager,
    IValidator<CreateArtistRequest> validator,
    IValidator<string> idValidator) : IArtistService
{
    public async Task<Result<ArtistResponse>> CreateAsync(CreateArtistRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await userManager.Users
            .SingleOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user is null)
            return Result.Fail<ArtistResponse>(new NotFoundError("User was not found."));

        if (string.IsNullOrWhiteSpace(user.Email) || user.Email.Length > 256)
            return Result.Fail<ArtistResponse>(new ConflictError("The user must have an email of at most 256 characters."));

        if (await artistRepository.Store.AnyAsync(a => a.UserId == user.Id, cancellationToken))
            return Result.Fail<ArtistResponse>(new ConflictError("The user already has an artist profile."));

        var artist = new Artist
        {
            UserId = user.Id,
            Name = request.Name.Trim(),
            EmailSnapshot = user.Email,
            Country = request.Country?.Trim(),
        };

        var result = await artistRepository.CreateAsync(artist, cancellationToken);
        if (result.IsFailed)
            return Result.Fail<ArtistResponse>(result.Errors);

        await artistRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok(new ArtistResponse(artist.Id, artist.Name, artist.EmailSnapshot, artist.Country));
    }

    public async Task<IReadOnlyList<ArtistResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await artistRepository.Store
            .OrderBy(a => a.Name).ThenBy(a => a.Id)
            .Select(a => new ArtistResponse(a.Id, a.Name, a.EmailSnapshot, a.Country))
            .ToListAsync(cancellationToken);
    }

    public async Task<Result<ArtistResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        await idValidator.ValidateAndThrowAsync(id, cancellationToken);

        var result = await artistRepository.GetByIdAsync(id, cancellationToken);
        if (result.IsFailed)
            return Result.Fail<ArtistResponse>(result.Errors);

        var artist = result.Value;
        return Result.Ok(new ArtistResponse(artist.Id, artist.Name, artist.EmailSnapshot, artist.Country));
    }
}

using Application.Features.Artists.Dtos;
using Common.Errors;
using Domain.Entities.Artists;
using Domain.Entities.Identity;
using Domain.Enums;
using FluentResults;
using FluentValidation;
using Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Artists;

public sealed class ArtistService(
    IArtistRepository artistRepository,
    UserManager<User> userManager,
    SqlServerDatabaseContext databaseContext,
    IValidator<CreateArtistRequest> validator,
    IValidator<string> idValidator) : IArtistService
{
    public async Task<Result<ArtistResponse>> CreateAsync(CreateArtistRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var user = new User
        {
            UserName = request.UserName.Trim(),
            Email = request.Email.Trim(),
            EmailConfirmed = true,
        };

        await using var transaction = await databaseContext.Database.BeginTransactionAsync(cancellationToken);
        var userResult = await userManager.CreateAsync(user, request.Password);
        if (!userResult.Succeeded)
            return Result.Fail<ArtistResponse>(ToIdentityError(userResult));

        var roleResult = await userManager.AddToRoleAsync(user, Roles.Artist.ToString());
        if (!roleResult.Succeeded)
            return Result.Fail<ArtistResponse>(ToIdentityError(roleResult));

        var artist = new Artist
        {
            UserId = user.Id,
            Name = request.Name.Trim(),
            EmailSnapshot = user.Email!,
            Country = string.IsNullOrWhiteSpace(request.Country) ? null : request.Country.Trim(),
        };

        var result = await artistRepository.CreateAsync(artist, cancellationToken);
        if (result.IsFailed)
            return Result.Fail<ArtistResponse>(result.Errors);

        await artistRepository.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Ok(new ArtistResponse(artist.Id, artist.Name, artist.EmailSnapshot, artist.Country));
    }

    private static IError ToIdentityError(IdentityResult result)
    {
        var message = string.Join(" ", result.Errors.Select(error => error.Description));
        return result.Errors.Any(error => error.Code is "DuplicateEmail" or "DuplicateUserName")
            ? new ConflictError(message)
            : new Error(message);
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

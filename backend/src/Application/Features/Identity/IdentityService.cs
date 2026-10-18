using Application.Features.Identity.Dtos;
using Common.Errors;
using Common.Options;
using Domain.Entities.Artists;
using Domain.Entities.Identity;
using Domain.Enums;
using FluentResults;
using FluentValidation;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Features.Identity;

public sealed class IdentityService(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    RoleManager<Role> roleManager,
    IArtistRepository artistRepository,
    IOptions<AppJwtBearerOptions> jwtOptions,
    IOptions<DemoOptions> demoOptions,
    TimeProvider timeProvider,
    IValidator<LoginRequest> validator,
    IValidator<string> idValidator) : IIdentityService
{
    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || !user.IsActive || user.RemovedAt is not null)
            return Result.Fail<LoginResponse>("Unable to sign in with these credentials.");

        if (!demoOptions.Value.Enabled && DemoAccounts.Accounts.Any(a => a.Id == user.Id))
            return Result.Fail<LoginResponse>("Unable to sign in with these credentials.");

        var signIn = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!signIn.Succeeded || await userManager.GetTwoFactorEnabledAsync(user))
            return Result.Fail<LoginResponse>("Unable to sign in with these credentials.");

        cancellationToken.ThrowIfCancellationRequested();
        var currentUser = await CreateUserResponseAsync(user, cancellationToken);
        var options = jwtOptions.Value;
        var now = timeProvider.GetUtcNow();
        var expiresAt = now.AddMinutes(options.ExpirationInMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id),
        };
        claims.AddRange(currentUser.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // Only identity and role claims are issued; stored claims can contain legacy credentials.
        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey)), SecurityAlgorithms.HmacSha256));

        return Result.Ok(new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), "Bearer", expiresAt, currentUser));
    }

    public async Task<Result<UserResponse>> GetCurrentUserAsync(string id, CancellationToken cancellationToken = default)
    {
        await idValidator.ValidateAndThrowAsync(id, cancellationToken);

        var user = await userManager.Users.SingleOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null || !user.IsActive || user.RemovedAt is not null)
            return Result.Fail<UserResponse>(new NotFoundError("User was not found."));

        return Result.Ok(await CreateUserResponseAsync(user, cancellationToken));
    }

    private async Task<UserResponse> CreateUserResponseAsync(User user, CancellationToken cancellationToken)
    {
        var assignedRoles = await userManager.GetRolesAsync(user);
        var roles = await roleManager.Roles.Where(r => assignedRoles.Contains(r.Name!))
            .OrderBy(r => r.Name).Select(r => r.Name!)
            .ToListAsync(cancellationToken);
        var artistId = await artistRepository.Store.Where(a => a.UserId == user.Id)
            .OrderBy(a => a.Id).Select(a => a.Id)
            .FirstOrDefaultAsync(cancellationToken);
        var canManageCatalog = roles.Contains(Roles.SuperAdmin.ToString()) || roles.Contains(Roles.Admin.ToString());

        return new UserResponse(user.Id, user.UserName ?? string.Empty, user.Email ?? string.Empty, roles, artistId, canManageCatalog);
    }
}

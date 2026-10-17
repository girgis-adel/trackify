using Application.Features.Identity.Dtos;
using FluentResults;

namespace Application.Features.Identity;

public interface IIdentityService
{
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<Result<UserResponse>> GetCurrentUserAsync(string id, CancellationToken cancellationToken = default);
}

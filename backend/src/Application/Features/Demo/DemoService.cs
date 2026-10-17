using Application.Features.Demo.Dtos;
using Common.Errors;
using Common.Options;
using FluentResults;
using Infrastructure.Identity;
using Microsoft.Extensions.Options;

namespace Application.Features.Demo;

public sealed class DemoService(IOptions<DemoOptions> demoOptions) : IDemoService
{
    public Task<Result<IReadOnlyList<DemoAccountResponse>>> GetAccountsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!demoOptions.Value.Enabled)
            return Task.FromResult(Result.Fail<IReadOnlyList<DemoAccountResponse>>(new NotFoundError("Demonstration accounts are unavailable.")));

        IReadOnlyList<DemoAccountResponse> accounts = DemoAccounts.Accounts
            .Select(a => new DemoAccountResponse(a.Role.ToString(), a.DisplayName, a.Email, DemoAccounts.Password, a.Description))
            .ToList();

        return Task.FromResult(Result.Ok(accounts));
    }
}

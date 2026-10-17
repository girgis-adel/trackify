using Application.Features.Demo.Dtos;
using FluentResults;

namespace Application.Features.Demo;

public interface IDemoService
{
    Task<Result<IReadOnlyList<DemoAccountResponse>>> GetAccountsAsync(CancellationToken cancellationToken = default);
}

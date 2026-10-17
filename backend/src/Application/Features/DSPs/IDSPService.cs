using Application.Features.DSPs.Dtos;

namespace Application.Features.DSPs;

public interface IDSPService
{
    Task<IReadOnlyList<DSPResponse>> GetAllAsync(CancellationToken cancellationToken = default);
}

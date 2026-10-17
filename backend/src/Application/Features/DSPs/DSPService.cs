using Application.Features.DSPs.Dtos;
using Domain.Entities.DSPs;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.DSPs;

public sealed class DSPService(IDSPRepository dspRepository) : IDSPService
{
    public async Task<IReadOnlyList<DSPResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dspRepository.Store.OrderBy(d => d.Name).ThenBy(d => d.Id)
            .Select(d => new DSPResponse(d.Id, d.Name))
            .ToListAsync(cancellationToken);
    }
}

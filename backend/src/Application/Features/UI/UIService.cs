using Application.Features.DSPs;
using Application.Features.UI.Dtos;
using Common.Options;
using Domain.Entities.Tracks;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Application.Features.UI;

public sealed class UIService(
    ITrackRepository trackRepository,
    IDSPService dspService,
    IOptions<DemoOptions> demoOptions) : IUIService
{
    public async Task<BootstrapResponse> GetBootstrapAsync(CancellationToken cancellationToken = default)
    {
        var genres = await trackRepository.Store.Select(t => t.Genre).Distinct()
            .OrderBy(genre => genre).ToListAsync(cancellationToken);
        var dsps = await dspService.GetAllAsync(cancellationToken);

        return new BootstrapResponse(genres, Enum.GetNames<TrackStatus>(), Enum.GetNames<TrackDistributionStatus>(), dsps, demoOptions.Value.Enabled, true);
    }
}

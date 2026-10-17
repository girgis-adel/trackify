using Application.Features.DSPs.Dtos;

namespace Application.Features.UI.Dtos;

public sealed record BootstrapResponse(
    IReadOnlyList<string> Genres,
    IReadOnlyList<string> TrackStatuses,
    IReadOnlyList<string> DistributionStatuses,
    IReadOnlyList<DSPResponse> DSPs,
    bool DemoEnabled,
    bool GuestBrowsingEnabled);

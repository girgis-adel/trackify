using Common.Core;
using Domain.Entities.DSPs;
using Domain.Entities.Tracks;
using Domain.Enums;

namespace Domain.Entities.TrackDistributions;

public sealed class TrackDistribution : Entity
{
    public TrackDistribution() : base() { }

    public TrackDistribution(string id) : base(id) { }

    public string TrackId { get; set; } = default!;

    public Track Track { get; set; } = default!;

    public string DSPId { get; set; } = default!;

    public DSP DSP { get; set; } = default!;

    public DateTime? GoLiveDate { get; set; }

    public TrackDistributionStatus Status { get; set; } = TrackDistributionStatus.Pending;
}

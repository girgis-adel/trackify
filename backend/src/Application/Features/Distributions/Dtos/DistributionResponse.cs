using Domain.Enums;

namespace Application.Features.Distributions.Dtos;

/// <summary>Submission information; SubmittedAt is the existing audited creation date.</summary>
public sealed record DistributionResponse(string Id, string DSPId, string DSPName, DateTimeOffset SubmittedAt, TrackDistributionStatus Status, DateTime? GoLiveDate);

using Application.Features.Distributions.Dtos;
using Domain.Enums;

namespace Application.Features.Tracks.Dtos;

/// <summary>A track and its DSP distribution statuses.</summary>
public sealed record TrackDetailsResponse(
    string Id,
    string Isrc,
    string Title,
    DateTime? ReleaseDate,
    string Genre,
    TrackStatus Status,
    IReadOnlyList<DistributionResponse> Distributions);

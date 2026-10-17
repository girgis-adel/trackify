namespace Application.Features.Distributions.Dtos;

/// <summary>DSP identifiers to which a track should be submitted.</summary>
public sealed record DistributeTrackRequest
{
    public string[] DSPIds { get; init; } = [];
}

using Domain.Enums;

namespace Application.Features.Tracks.Dtos;

/// <summary>Track catalog information using the existing domain date representation.</summary>
public sealed record TrackResponse(string Id, string Isrc, string Title, DateTime? ReleaseDate, string Genre, TrackStatus Status);

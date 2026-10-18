using Common.Core;
using Common.Core.Properties;
using Domain.Entities.Artists;
using Domain.Enums;

namespace Domain.Entities.Tracks;

public sealed class Track : RemovableEntity, IIsActive
{
    public Track() : base() { }

    public Track(string id) : base(id) { }

    public string Isrc { get; set; } = default!;

    public string Title { get; set; } = default!;

    public string ArtistId { get; set; } = default!;

    public Artist Artist { get; set; } = default!;

    public DateTime? ReleaseDate { get; set; }

    public string Genre { get; set; } = default!;

    public TrackStatus Status { get; set; } = TrackStatus.Draft;

    public bool IsActive { get; set; } = true;
}

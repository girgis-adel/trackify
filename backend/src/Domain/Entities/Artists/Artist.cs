using Common.Core;
using Common.Core.Properties;
using Domain.Entities.Identity;

namespace Domain.Entities.Artists;

public sealed class Artist : RemovableEntity, IIsActive
{
    public Artist() : base() { }

    public Artist(string id) : base(id) { }

    public string UserId { get; set; } = default!;

    public User User { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string EmailSnapshot { get; set; } = default!;

    public string? Country { get; set; }

    public bool IsActive { get; set; } = true;
}

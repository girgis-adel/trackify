using Common.Core;

namespace Domain.Entities.DSPs;

public sealed class DSP : Entity
{
    public DSP() : base() { }

    public DSP(string id) : base(id) { }

    public string Name { get; set; } = default!;

    public string NormalizedName { get; set; } = default!;
}

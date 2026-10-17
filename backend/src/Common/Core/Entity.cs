using Common.Core.Abstractions;

namespace Common.Core;

public abstract class Entity(string id) : IEntity
{
    protected Entity() : this(Guid.CreateVersion7().ToString()) { }

    public virtual string Id { get; set; } = id;

    public virtual DateTimeOffset CreateDate { get; set; }
    
    public virtual DateTimeOffset UpdateDate { get; set; }

    public virtual byte[] Version { get; set; } = default!;
}

using Common.Core.Abstractions;

namespace Common.Core;

public abstract class RemovableEntity(string id) : Entity(id), IRemovableEntity
{
    protected RemovableEntity() : this(Guid.CreateVersion7().ToString()) { }
    
    public virtual DateTimeOffset? RemovedAt { get; set; }

    public virtual bool IsRemovable { get; set; } = true;
}

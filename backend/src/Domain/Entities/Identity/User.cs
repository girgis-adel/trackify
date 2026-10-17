using Common.Core.Abstractions;
using Common.Core.Properties;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities.Identity;

public sealed class User : IdentityUser, IId, ICreateDate, IUpdateDate, ISoftRemovable, IIsActive
{
    public User() : base() { }

    public User(string userName) : base(userName) { }

    public override string Id { get; set; } = Guid.CreateVersion7().ToString();
    
    public DateTimeOffset CreateDate { get; set; }

    public DateTimeOffset UpdateDate { get; set; }
    
    public DateTimeOffset? RemovedAt { get; set; }
    
    public bool IsRemovable { get; set; } = true;

    public bool IsActive { get; set; } = true;
}

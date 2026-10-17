using Common.Core.Properties;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities.Identity;

public sealed class Role : IdentityRole, IId, ICreateDate, IUpdateDate, IIsActive
{
    public Role() : base() { }

    public Role(string roleName) : base(roleName) { }

    public override string Id { get; set; } = Guid.CreateVersion7().ToString();

    public DateTimeOffset CreateDate { get; set; }

    public DateTimeOffset UpdateDate { get; set; }

    public bool IsActive { get; set; } = true;
}

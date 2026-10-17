using Domain.Enums;

namespace Api.Controllers.V1.Infrastructure;

public static class CatalogRoles
{
    public const string Managers = nameof(Roles.SuperAdmin) + "," + nameof(Roles.Admin);
}

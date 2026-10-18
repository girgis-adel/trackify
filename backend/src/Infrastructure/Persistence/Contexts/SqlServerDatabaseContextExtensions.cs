using Common.Core.Abstractions;
using Common.Core.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;

namespace Infrastructure.Persistence.Contexts;

public static class SqlServerDatabaseContextExtensions
{
    public static void ApplySoftRemovableQueryFilters(this ModelBuilder builder)
    {
        var entityTypes = builder.Model
            .GetEntityTypes()
            .Where(entityType => entityType.BaseType == null && !entityType.IsOwned() &&
                    typeof(ISoftRemovable).IsAssignableFrom(entityType.ClrType));

        foreach (var entityType in entityTypes)
        {
            var parameter = Expression.Parameter(entityType.ClrType, "e");

            var isRemovable = Expression.Property(
                parameter, nameof(ISoftRemovable.IsRemovable));

            var removedAt = Expression.Property(
                parameter, nameof(ISoftRemovable.RemovedAt));

            // !IsRemovable || (RemovedAt == null)
            var body = Expression.OrElse(
                Expression.Not(isRemovable),
                Expression.Equal(removedAt, Expression.Constant(null, typeof(DateTimeOffset?))));

            var filter = Expression.Lambda(body, parameter);

            builder.Entity(entityType.ClrType)
                .HasQueryFilter(QueryFilterKeys.SoftRemovableFilter, filter);
        }
    }

    public static void ApplyIsActiveQueryFilters(this ModelBuilder builder)
    {
        var entityTypes = builder.Model
            .GetEntityTypes()
            .Where(entityType => entityType.BaseType == null && !entityType.IsOwned() &&
                    typeof(IIsActive).IsAssignableFrom(entityType.ClrType));

        foreach (var entityType in entityTypes)
        {
            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var isActive = Expression.Property(parameter, nameof(IIsActive.IsActive));
            var body = Expression.Equal(isActive, Expression.Constant(true));
            var filter = Expression.Lambda(body, parameter);
            builder.Entity(entityType.ClrType).HasQueryFilter(QueryFilterKeys.IsActiveFilter, filter);
        }
    }

    public static void ApplyCreateDateProtectionBehavior(this ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (!typeof(ICreateDate).IsAssignableFrom(entityType.ClrType))
                continue;
            var property = entityType.FindProperty(nameof(ICreateDate.CreateDate));
            property?.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        }
    }

    public static void ConfigureVersion(this ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(IVersion).IsAssignableFrom(entityType.ClrType))
            {
                var property = entityType.FindProperty(nameof(IVersion.Version));
                if (property != null && property.ClrType == typeof(byte[]))
                {
                    builder.Entity(entityType.ClrType)
                        .Property(nameof(IVersion.Version))
                        .IsRowVersion();
                }
            }
        }
    }
}

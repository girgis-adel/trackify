using Common.Core.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Persistence.Interceptors;

public sealed class AuditableInterceptor(TimeProvider timeProvider) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Apply(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Apply(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Apply(DbContext? context)
    {
        if (context is null)
            return;

        var now = timeProvider.GetUtcNow();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity is ICreateDate)
                    entry.Property(nameof(ICreateDate.CreateDate))
                        .CurrentValue = now;

                if (entry.Entity is IUpdateDate)
                    entry.Property(nameof(IUpdateDate.UpdateDate))
                        .CurrentValue = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                if (entry.Entity is IUpdateDate)
                    entry.Property(nameof(IUpdateDate.UpdateDate))
                        .CurrentValue = now;
            }
        }
    }
}

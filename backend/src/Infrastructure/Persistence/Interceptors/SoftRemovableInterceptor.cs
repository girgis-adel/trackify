using Common.Core.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Persistence.Interceptors;

public sealed class SoftRemovableInterceptor(TimeProvider timeProvider) : SaveChangesInterceptor
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
        if (context is null) return;

        var entries = context.ChangeTracker
            .Entries<ISoftRemovable>()
            .Where(e => e.State == EntityState.Deleted && e.Entity.IsRemovable)
            .ToList();

        if (entries.Count == 0) return;

        var now = timeProvider.GetUtcNow();

        foreach (var entry in entries)
        {
            var isRemovable = entry.Property(nameof(ISoftRemovable.IsRemovable)).CurrentValue as bool?;
            if (isRemovable is not null && isRemovable is true)
            {
                entry.State = EntityState.Modified;
                entry.Property(nameof(ISoftRemovable.RemovedAt)).CurrentValue = now;
                entry.Property(nameof(ISoftRemovable.RemovedAt)).IsModified = true;
            }
        }
    }
}

using Common.Abstractions;
using Common.Errors;
using FluentResults;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class Repository<TEntity>(SqlServerDatabaseContext databaseContext) : IRepository<TEntity>
    where TEntity : class
{
    public IQueryable<TEntity> Store => databaseContext.Set<TEntity>();

    public bool AutoSaveChanges { get; set; }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await databaseContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Result> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        databaseContext.Set<TEntity>().Add(entity);

        if (AutoSaveChanges)
            await SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        databaseContext.Entry(entity).State = EntityState.Modified;

        if (AutoSaveChanges)
            await SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        databaseContext.Set<TEntity>().Remove(entity);

        if (AutoSaveChanges)
            await SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result<TEntity>> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await Store.AsNoTracking()
            .SingleOrDefaultAsync(e => EF.Property<string>(e, "Id") == id, cancellationToken);

        return entity is null
            ? Result.Fail<TEntity>(new NotFoundError($"{typeof(TEntity).Name} was not found."))
            : Result.Ok(entity);
    }

    public async Task<Result<TEntity>> FindByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        // Query the store even when tracked so that global filters remain effective.
        var entity = await Store
            .SingleOrDefaultAsync(e => EF.Property<string>(e, "Id") == id, cancellationToken);

        return entity is null
            ? Result.Fail<TEntity>(new NotFoundError($"{typeof(TEntity).Name} was not found."))
            : Result.Ok(entity);
    }

    public void Dispose()
    {
        // The dependency injection scope owns the shared database context.
        GC.SuppressFinalize(this);
    }
}

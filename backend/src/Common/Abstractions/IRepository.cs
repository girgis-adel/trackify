using FluentResults;

namespace Common.Abstractions;

public interface IRepository<TEntity> : IDisposable
    where TEntity : class
{
    IQueryable<TEntity> Store { get; }

    bool AutoSaveChanges { get; set; }

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<Result> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<Result> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<Result<TEntity>> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<Result<TEntity>> FindByIdAsync(string id, CancellationToken cancellationToken = default);
}

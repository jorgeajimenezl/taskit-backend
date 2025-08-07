namespace Taskit.Application.Interfaces;

public interface IRepository<TEntity, TKey> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(
        TEntity entity,
        bool saveChanges = true,
        CancellationToken cancellationToken = default);

    Task AddRangeAsync(
        IEnumerable<TEntity> entities,
        bool saveChanges = true,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        TEntity entity,
        bool saveChanges = true,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        TKey id,
        bool saveChanges = true,
        CancellationToken cancellationToken = default);

    Task DeleteRangeAsync(
        IEnumerable<TEntity> entities,
        bool saveChanges = true,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default);

    IQueryable<TEntity> Query();

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
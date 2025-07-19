namespace Taskit.Application.Interfaces;

public interface IRepository<TEntity, TKey> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(TKey id);
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task AddAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(TKey id);
    Task DeleteRangeAsync(IEnumerable<TEntity> entities);
    Task<bool> ExistsAsync(TKey id);
    IQueryable<TEntity> Query();
}
namespace Taskit.Application.Interfaces;

public interface IRepository<TEntity, TKey> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(TKey id);
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task AddAsync(TEntity entity, bool saveChanges = true);
    Task UpdateAsync(TEntity entity, bool saveChanges = true);
    Task DeleteAsync(TKey id, bool saveChanges = true);
    Task DeleteRangeAsync(IEnumerable<TEntity> entities, bool saveChanges = true);
    Task<bool> ExistsAsync(TKey id);
    IQueryable<TEntity> Query();
    Task<int> SaveChangesAsync();
}
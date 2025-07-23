using Microsoft.EntityFrameworkCore;
using Taskit.Application.Interfaces;

namespace Taskit.Infrastructure.Repositories;

public class Repository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(TKey id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task AddAsync(TEntity entity, bool saveChanges = true)
    {
        await _dbSet.AddAsync(entity);
        if (saveChanges)
            await _context.SaveChangesAsync();
    }

    public virtual async Task UpdateAsync(TEntity entity, bool saveChanges = true)
    {
        _dbSet.Update(entity);
        if (saveChanges)
            await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(TKey id, bool saveChanges = true)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            if (saveChanges)
                await _context.SaveChangesAsync();
        }
    }

    public virtual async Task DeleteRangeAsync(IEnumerable<TEntity> entities, bool saveChanges = true)
    {
        _dbSet.RemoveRange(entities);
        if (saveChanges)
            await _context.SaveChangesAsync();
    }

    public virtual async Task<bool> ExistsAsync(TKey id)
    {
        var entity = await GetByIdAsync(id);
        return entity != null;
    }

    public virtual IQueryable<TEntity> Query()
    {
        return _dbSet.AsQueryable();
    }

    public Task<int> SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
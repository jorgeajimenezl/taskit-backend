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

    public virtual async Task<TEntity?> GetByIdAsync(
        TKey id,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync([id], cancellationToken: cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken: cancellationToken);
    }

    public virtual async Task AddAsync(
        TEntity entity,
        bool saveChanges = true,
        CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        if (saveChanges)
            await _context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task AddRangeAsync(
        IEnumerable<TEntity> entities,
        bool saveChanges = true,
        CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
        if (saveChanges)
            await _context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task UpdateAsync(
        TEntity entity,
        bool saveChanges = true,
        CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        if (saveChanges)
            await _context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync(
        TKey id,
        bool saveChanges = true,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            if (saveChanges)
                await _context.SaveChangesAsync();
        }
    }

    public virtual async Task DeleteRangeAsync(
        IEnumerable<TEntity> entities,
        bool saveChanges = true,
        CancellationToken cancellationToken = default)
    {
        _dbSet.RemoveRange(entities);
        if (saveChanges)
            await _context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(
        TKey id,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        return entity != null;
    }

    public virtual IQueryable<TEntity> Query()
    {
        return _dbSet.AsQueryable();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
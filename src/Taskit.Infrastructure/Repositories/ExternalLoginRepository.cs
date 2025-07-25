using Microsoft.EntityFrameworkCore;
using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;

namespace Taskit.Infrastructure.Repositories;

public class ExternalLoginRepository(AppDbContext context)
    : Repository<ExternalLogin, Guid>(context), IExternalLoginRepository
{
    public async Task<ExternalLogin?> GetByProviderAsync(string provider, string providerUserId)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Provider == provider && e.ProviderUserId == providerUserId);
    }

    public async Task<IEnumerable<ExternalLogin>> GetByUserIdAsync(string userId)
    {
        return await _dbSet.Where(e => e.UserId == userId).ToListAsync();
    }
}
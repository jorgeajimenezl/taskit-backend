using Microsoft.EntityFrameworkCore;
using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;
using Taskit.Infrastructure;

namespace Taskit.Infrastructure.Repositories;

public class RefreshTokenRepository(AppDbContext context) : Repository<RefreshToken, Guid>(context), IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByTokenAsync(string tokenHash)
    {
        return await _dbSet.Include(r => r.User)
            .FirstOrDefaultAsync(r => r.TokenHash == tokenHash);
    }
}

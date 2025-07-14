using Microsoft.EntityFrameworkCore;
using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;
using Taskit.Infrastructure;

namespace Taskit.Infrastructure.Repositories;

public class RefreshTokenRepository(AppDbContext context) : Repository<RefreshToken, int>(context), IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _dbSet.Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == token);
    }
}

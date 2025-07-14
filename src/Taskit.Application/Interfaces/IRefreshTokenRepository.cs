namespace Taskit.Application.Interfaces;

using Taskit.Domain.Entities;

public interface IRefreshTokenRepository : IRepository<RefreshToken, int>
{
    Task<RefreshToken?> GetByTokenAsync(string token);
}

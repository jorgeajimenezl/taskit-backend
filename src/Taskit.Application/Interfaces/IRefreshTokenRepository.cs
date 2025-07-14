namespace Taskit.Application.Interfaces;

using Taskit.Domain.Entities;

public interface IRefreshTokenRepository : IRepository<RefreshToken, Guid>
{
    Task<RefreshToken?> GetByTokenAsync(string tokenHash);
}

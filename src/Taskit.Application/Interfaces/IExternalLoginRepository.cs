namespace Taskit.Application.Interfaces;

using Taskit.Domain.Entities;

public interface IExternalLoginRepository : IRepository<ExternalLogin, Guid>
{
    Task<ExternalLogin?> GetByProviderAsync(string provider, string providerUserId);
    Task<IEnumerable<ExternalLogin>> GetByUserIdAsync(string userId);
}


using Taskit.Domain.Entities;

namespace Taskit.Application.Interfaces;

public interface IProjectActivityLogRepository : IRepository<ProjectActivityLog, int>
{
    IQueryable<ProjectActivityLog> QueryForUser(string userId);
}

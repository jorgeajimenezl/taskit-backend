using Taskit.Domain.Entities;
using Taskit.Application.Interfaces;

namespace Taskit.Infrastructure.Repositories;

public class ProjectActivityLogRepository(AppDbContext context) : Repository<ProjectActivityLog, int>(context), IProjectActivityLogRepository
{
    public IQueryable<ProjectActivityLog> QueryForUser(string userId)
    {
        return Query().Where(a =>
            a.Project != null &&
            (a.Project.OwnerId == userId || a.Project.Members.Any(m => m.UserId == userId)));
    }
}

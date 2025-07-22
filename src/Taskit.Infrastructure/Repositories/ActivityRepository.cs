using Taskit.Domain.Entities;
using Taskit.Application.Interfaces;

namespace Taskit.Infrastructure.Repositories;

public class ActivityRepository(AppDbContext context) : Repository<Activity, int>(context), IActivityRepository
{
    public IQueryable<Activity> QueryForUser(string userId)
    {
        return Query().Where(a =>
            a.Project != null &&
            (a.Project.OwnerId == userId || a.Project.Members.Any(m => m.UserId == userId)));
    }
}

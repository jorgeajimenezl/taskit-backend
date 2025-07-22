using Taskit.Domain.Entities;

namespace Taskit.Application.Interfaces;

public interface IActivityRepository : IRepository<Activity, int>
{
    IQueryable<Activity> QueryForUser(string userId);
}

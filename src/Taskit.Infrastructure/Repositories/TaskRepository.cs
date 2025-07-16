using Taskit.Domain.Entities;
using Taskit.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Taskit.Infrastructure.Repositories;

public class TaskRepository(AppDbContext context) : Repository<AppTask, int>(context), ITaskRepository
{
    public IQueryable<AppTask> QueryForUser(string userId)
    {
        return Query().Where(t =>
            t.AuthorId == userId ||
            t.AssignedUserId == userId ||
            (t.Project != null &&
                (t.Project.OwnerId == userId || t.Project.Members.Any(m => m.UserId == userId))));
    }
}

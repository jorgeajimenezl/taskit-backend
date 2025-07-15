using Taskit.Domain.Entities;
using Taskit.Application.Interfaces;
using Taskit.Application.Common.Models;
using Taskit.Application.Common.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Taskit.Infrastructure.Repositories;

public class TaskRepository(AppDbContext context) : Repository<AppTask, int>(context), ITaskRepository
{
    public override IQueryable<AppTask> Query()
    {
        return base.Query()
            .Include(t => t.Project)
            .ThenInclude(p => p.Members);
    }

    public IQueryable<AppTask> QueryForUser(string userId)
    {
        return Query().Where(t =>
            t.AuthorId == userId ||
            t.AssignedUserId == userId ||
            (t.Project != null &&
                (t.Project.OwnerId == userId || t.Project.Members.Any(m => m.UserId == userId))));
    }
}

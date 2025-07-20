using System.Linq;
using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;

namespace Taskit.Infrastructure.Repositories;

public class TaskCommentRepository(AppDbContext context) : Repository<TaskComment, int>(context), ITaskCommentRepository
{
    public IQueryable<TaskComment> QueryForTask(int taskId)
    {
        return Query().Where(c => c.TaskId == taskId);
    }
}

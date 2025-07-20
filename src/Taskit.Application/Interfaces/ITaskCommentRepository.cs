using Taskit.Domain.Entities;

namespace Taskit.Application.Interfaces;

public interface ITaskCommentRepository : IRepository<TaskComment, int>
{
    IQueryable<TaskComment> QueryForTask(int taskId);
}

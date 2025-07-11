using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;

namespace Taskit.Application.Interfaces;

public interface ITaskRepository : IRepository<AppTask, int>
{
    Task<IEnumerable<AppTask>> GetTasksByIdAsync(string userId);
    Task<IEnumerable<AppTask>> GetTasksByProjectIdAsync(int projectId);
}
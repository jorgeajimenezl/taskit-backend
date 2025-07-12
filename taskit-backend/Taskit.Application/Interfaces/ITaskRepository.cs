using Taskit.Application.Common.Models;
using Taskit.Domain.Entities;

namespace Taskit.Application.Interfaces;

public interface ITaskRepository : IRepository<AppTask, int>
{
    public Task<PaginatedList<AppTask>> GetTasksByAssignedUserIdAsync(string assignedUserId, int pageIndex, int pageSize);
}
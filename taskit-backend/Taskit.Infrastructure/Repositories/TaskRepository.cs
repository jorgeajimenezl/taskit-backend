using Taskit.Domain.Entities;
using Taskit.Application.Interfaces;
using Taskit.Application.Common.Models;
using Taskit.Application.Common.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Taskit.Infrastructure.Repositories;

public class TaskRepository(AppDbContext context) : Repository<AppTask, int>(context), ITaskRepository
{
    public async Task<PaginatedList<AppTask>> GetTasksByAssignedUserIdAsync(string assignedUserId, int pageIndex, int pageSize)
    {
        var query = _context.Tasks
            .Where(t => t.AssignedUserId == assignedUserId)
            .OrderBy(t => t.Id);

        return await query.PaginatedListAsync(pageIndex, pageSize);
    }
}
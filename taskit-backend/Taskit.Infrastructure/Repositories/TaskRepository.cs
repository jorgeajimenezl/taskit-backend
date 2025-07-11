using Microsoft.EntityFrameworkCore;
using Taskit.Domain.Entities;

namespace Taskit.Infrastructure.Repositories;

class TaskRepository(AppDbContext context) : Repository<Task>(context)
{
    public async Task<IEnumerable<AppTask>> GetTasksByProjectIdAsync(int projectId)
    {
        return await _context.Tasks
            .Where(t => t.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<IEnumerable<AppTask>> GetTasksByIdAsync(string userId)
    {
        return await _context.Tasks
            .Where(t => t.AssignedUserId == userId)
            .ToListAsync();
    }
}
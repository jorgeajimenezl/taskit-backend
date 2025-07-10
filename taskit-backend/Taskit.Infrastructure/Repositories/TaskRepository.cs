using Microsoft.EntityFrameworkCore;

namespace Taskit.Infrastructure.Repositories;

class TaskRepository(AppDbContext context) : Repository<Task>(context)
{

    // public async Task<IEnumerable<Task>> GetTasksByProjectIdAsync(int projectId)
    // {
    //     return await _context.Tasks
    //         .Where(t => t.ProjectId == projectId)
    //         .ToListAsync();
    // }

    // public async Task<IEnumerable<Task>> GetTasksByIdAsync(int userId)
    // {
    //     return await _context.Tasks
    //         .Where(t => t.AssignedUserId == userId)
    //         .ToListAsync();
    // }
}
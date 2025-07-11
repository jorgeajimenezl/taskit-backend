using Microsoft.EntityFrameworkCore;
using Taskit.Domain.Entities;
using Taskit.Application.Interfaces;

namespace Taskit.Infrastructure.Repositories;

public class TaskRepository(AppDbContext context) : Repository<Task, int>(context)
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
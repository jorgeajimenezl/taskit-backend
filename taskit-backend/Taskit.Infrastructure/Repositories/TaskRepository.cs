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
        // TODO:Check access to the tasks
        return base.Query();
    }
}
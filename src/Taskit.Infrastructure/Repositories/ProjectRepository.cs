using Microsoft.EntityFrameworkCore;
using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;

namespace Taskit.Infrastructure.Repositories;

public class ProjectRepository(AppDbContext context) : Repository<Project, int>(context), IProjectRepository
{
    public override IQueryable<Project> Query()
    {
        return base.Query().Include(p => p.Members);
    }
}

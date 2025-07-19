using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;

namespace Taskit.Infrastructure.Repositories;

public class ProjectMemberRepository(AppDbContext context) : Repository<ProjectMember, int>(context), IProjectMemberRepository
{
    public IQueryable<ProjectMember> QueryForProject(int projectId)
    {
        return Query().Where(pm => pm.ProjectId == projectId);
    }
}

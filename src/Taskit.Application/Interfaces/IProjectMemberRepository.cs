using Taskit.Domain.Entities;

namespace Taskit.Application.Interfaces;

public interface IProjectMemberRepository : IRepository<ProjectMember, int>
{
    IQueryable<ProjectMember> QueryForProject(int projectId);
}

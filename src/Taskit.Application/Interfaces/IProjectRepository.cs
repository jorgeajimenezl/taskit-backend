using Taskit.Domain.Entities;

namespace Taskit.Application.Interfaces;

public interface IProjectRepository : IRepository<Project, int>
{
    /// <summary>
    /// Returns a queryable with project members included. Use this when member
    /// information is required.
    /// </summary>
    IQueryable<Project> QueryWithMembers();
}

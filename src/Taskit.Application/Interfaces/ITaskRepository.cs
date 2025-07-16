using Taskit.Application.Common.Models;
using Taskit.Domain.Entities;

namespace Taskit.Application.Interfaces;

public interface ITaskRepository : IRepository<AppTask, int>
{
    IQueryable<AppTask> QueryForUser(string userId);
}
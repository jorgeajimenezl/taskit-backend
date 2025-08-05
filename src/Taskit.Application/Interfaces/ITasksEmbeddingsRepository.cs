using Taskit.Application.Common.Models;
using Taskit.Domain.Entities;

namespace Taskit.Application.Interfaces;

public interface ITasksEmbeddingsRepository : IRepository<TaskEmbeddings, int>
{
}
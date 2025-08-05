using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;
using Taskit.Infrastructure;
using Taskit.Infrastructure.Repositories;

namespace Taskit.Infrastructure.Repositories;

public class TasksEmbeddingsRepository(AppDbContext context) : Repository<TaskEmbeddings, int>(context), ITasksEmbeddingsRepository
{

}
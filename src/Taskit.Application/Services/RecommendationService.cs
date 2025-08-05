using Taskit.Application.Interfaces;

namespace Taskit.Application.Services;

public class RecommendationService(ITasksEmbeddingsRepository tasksEmbeddingsRepository)
{
    private readonly ITasksEmbeddingsRepository _tasksEmbeddingsRepository = tasksEmbeddingsRepository;
}
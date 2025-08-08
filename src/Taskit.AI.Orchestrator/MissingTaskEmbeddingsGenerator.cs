using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Embeddings;
using Pgvector;
using Taskit.AI.Orchestrator.Settings;
using Taskit.Domain.Entities;
using Taskit.Infrastructure;

namespace Taskit.AI.Orchestrator;

public class MissingTaskEmbeddingsGenerator(
    OpenAIClient openAiClient,
    AppDbContext db,
    IOptions<EmbeddingsGeneratorSettings> settings,
    ILogger<MissingTaskEmbeddingsGenerator> logger)
{
    private readonly OpenAIClient _openAiClient = openAiClient;
    private readonly AppDbContext _db = db;
    private readonly EmbeddingsGeneratorSettings _settings = settings.Value;
    private readonly ILogger<MissingTaskEmbeddingsGenerator> _logger = logger;

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var client = _openAiClient.GetEmbeddingClient(_settings.Model);

        var tasks = await _db.Tasks
            .Where(t => !_db.Set<TaskEmbeddings>()
                .Any(e => e.TaskId == t.Id && (e.DescriptionEmbedding != null || e.TitleEmbedding != null)))
            .ToListAsync(cancellationToken);

        foreach (var task in tasks)
        {
            Vector? descEmbedding = null;
            if (!string.IsNullOrWhiteSpace(task.Description))
            {
                var resp = await client.GenerateEmbeddingAsync(task.Description, new EmbeddingGenerationOptions()
                {
                    Dimensions = 1536,
                }, cancellationToken: cancellationToken);
                descEmbedding = new Vector(resp.Value.ToFloats());
            }

            Vector? titleEmbedding = null;
            if (!string.IsNullOrWhiteSpace(task.Title))
            {
                var resp = await client.GenerateEmbeddingAsync(task.Title, new EmbeddingGenerationOptions()
                {
                    Dimensions = 500,
                }, cancellationToken: cancellationToken);
                titleEmbedding = new Vector(resp.Value.ToFloats());
            }

            var existing = await _db.Set<TaskEmbeddings>().FirstOrDefaultAsync(e => e.TaskId == task.Id, cancellationToken);
            if (existing is null)
            {
                existing = new TaskEmbeddings { TaskId = task.Id };
                _db.Add(existing);
            }

            existing.DescriptionEmbedding = descEmbedding;
            existing.TitleEmbedding = titleEmbedding;

            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Generated embeddings for task {TaskId}", task.Id);
        }
    }
}


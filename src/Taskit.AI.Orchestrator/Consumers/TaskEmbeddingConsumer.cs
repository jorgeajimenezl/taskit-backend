using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Embeddings;
using Pgvector;
using Taskit.AI.Orchestrator.Settings;
using Taskit.Domain.Entities;
using Taskit.Domain.Enums;
using Taskit.Domain.Events;
using Taskit.Infrastructure;

namespace Taskit.AI.Orchestrator.Consumers;

public class TaskEmbeddingConsumer(
    OpenAIClient openAiClient,
    AppDbContext db,
    IOptions<EmbeddingsGeneratorSettings> settings,
    ILogger<TaskEmbeddingConsumer> logger) : IConsumer<ProjectActivityLogCreated>
{
    private readonly OpenAIClient _openAiClient = openAiClient;
    private readonly AppDbContext _db = db;
    private readonly EmbeddingsGeneratorSettings _settings = settings.Value;
    private readonly ILogger<TaskEmbeddingConsumer> _logger = logger;

    public async Task Consume(ConsumeContext<ProjectActivityLogCreated> context)
    {
        var evt = context.Message;
        if (evt.TaskId is null)
            return;

        var shouldHandle = evt.EventType switch
        {
            ProjectActivityLogEventType.TaskCreated => true,
            ProjectActivityLogEventType.TaskUpdated => evt.Data != null &&
                (evt.Data.ContainsKey("description") || evt.Data.ContainsKey("title")),
            _ => false
        };

        if (!shouldHandle)
            return;

        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == evt.TaskId, context.CancellationToken);
        if (task is null)
            return;

        try
        {
            var client = _openAiClient.GetEmbeddingClient(_settings.Model);

            Vector? descEmbedding = null;
            if (!string.IsNullOrWhiteSpace(task.Description))
            {
                var resp = await client.GenerateEmbeddingAsync(task.Description, cancellationToken: context.CancellationToken);
                var arr = resp.Value.ToFloats().ToArray();
                descEmbedding = new Vector(arr);
            }

            Vector? titleEmbedding = null;
            if (!string.IsNullOrWhiteSpace(task.Title))
            {
                var resp = await client.GenerateEmbeddingAsync(task.Title, cancellationToken: context.CancellationToken);
                var arr = resp.Value.ToFloats().ToArray();
                if (arr.Length > 500)
                {
                    Array.Resize(ref arr, 500);
                }
                titleEmbedding = new Vector(arr);
            }

            var existing = await _db.Set<TaskEmbeddings>().FirstOrDefaultAsync(e => e.TaskId == task.Id, context.CancellationToken);
            if (existing is null)
            {
                existing = new TaskEmbeddings { TaskId = task.Id };
                _db.Add(existing);
            }

            existing.DescriptionEmbedding = descEmbedding;
            existing.TitleEmbedding = titleEmbedding;

            await _db.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("Generated embeddings for task {TaskId}", task.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate embeddings for task {TaskId}", evt.TaskId);
        }
    }
}

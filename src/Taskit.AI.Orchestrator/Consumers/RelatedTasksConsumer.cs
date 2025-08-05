using MassTransit;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;
using Taskit.Domain.Entities;
using Taskit.Domain.Messages;
using Taskit.Infrastructure;

namespace Taskit.AI.Orchestrator.Consumers;

public class RelatedTasksConsumer(AppDbContext db) : IConsumer<RelatedTasksQuery>
{
    private readonly AppDbContext _db = db;

    public async Task Consume(ConsumeContext<RelatedTasksQuery> context)
    {
        var message = context.Message;
        var embedding = await _db.Set<TaskEmbeddings>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.TaskId == message.TaskId, context.CancellationToken);

        if (embedding is null || (embedding.DescriptionEmbedding is null && embedding.TitleEmbedding is null))
        {
            await context.RespondAsync(new RelatedTasksQueryResult(true, null));
            return;
        }

        IQueryable<TaskEmbeddings> query;
        if (embedding.DescriptionEmbedding is not null)
        {
            query = _db.Set<TaskEmbeddings>()
                .AsNoTracking()
                .Where(e => e.TaskId != message.TaskId && e.DescriptionEmbedding != null)
                .OrderBy(e => EF.Functions.CosineDistance(e.DescriptionEmbedding!, embedding.DescriptionEmbedding!));
        }
        else
        {
            query = _db.Set<TaskEmbeddings>()
                .AsNoTracking()
                .Where(e => e.TaskId != message.TaskId && e.TitleEmbedding != null)
                .OrderBy(e => EF.Functions.CosineDistance(e.TitleEmbedding!, embedding.TitleEmbedding!));
        }

        var relatedIds = await query
            .Select(e => e.TaskId)
            .Take(message.Count)
            .ToListAsync(context.CancellationToken);

        await context.RespondAsync(new RelatedTasksQueryResult(false, relatedIds));
    }
}

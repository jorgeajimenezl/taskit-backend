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
        var taskEmbd = await _db.Set<TaskEmbeddings>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.TaskId == message.TaskId, context.CancellationToken);

        if (taskEmbd is null || (taskEmbd.DescriptionEmbedding is null && taskEmbd.TitleEmbedding is null))
        {
            // TODO: probably the embeddings are still being generated
            // Respond with an in-progress status
            // Anyway, we need to handle this case
            await context.RespondAsync<IOperationInProgress>(new OperationInProgress(DateTime.UtcNow));
            return;
        }

        IQueryable<TaskEmbeddings> query;
        if (taskEmbd.DescriptionEmbedding is not null)
        {
            query = _db.Set<TaskEmbeddings>()
                .AsNoTracking()
                .Where(e => e.TaskId != message.TaskId && e.DescriptionEmbedding != null)
                .OrderBy(e => taskEmbd.DescriptionEmbedding!.CosineDistance(e.DescriptionEmbedding!));
        }
        else
        {
            query = _db.Set<TaskEmbeddings>()
                .AsNoTracking()
                .Where(e => e.TaskId != message.TaskId && e.TitleEmbedding != null)
                .OrderBy(e => taskEmbd.TitleEmbedding!.CosineDistance(e.TitleEmbedding!));
        }

        var relatedIds = await query
            .Select(e => e.TaskId)
            .Take(message.Count)
            .ToListAsync(context.CancellationToken);

        await context.RespondAsync(new OperationSucceeded<RelatedTasksQueryResult>(
            DateTime.UtcNow,
            new(relatedIds))
        );
    }
}

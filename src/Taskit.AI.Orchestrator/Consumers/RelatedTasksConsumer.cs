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
            await context.RespondAsync(OperationResult<RelatedTasksQueryResult>.Processing());
            return;
        }

        IQueryable<TaskEmbeddings> query;
        if (taskEmbd.DescriptionEmbedding is not null)
        {
            query = _db.Set<TaskEmbeddings>()
                .AsNoTracking()
                .Where(e => e.TaskId != message.TaskId && e.DescriptionEmbedding != null)
                .OrderBy(e => taskEmbd.TitleEmbedding!.CosineDistance(e.DescriptionEmbedding!));
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

        await context.RespondAsync(OperationResult<RelatedTasksQueryResult>.Success(new(relatedIds)));
    }
}

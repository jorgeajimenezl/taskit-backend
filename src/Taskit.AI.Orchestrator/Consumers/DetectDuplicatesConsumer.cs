using System;
using System.Linq;
using MassTransit;
using MassTransit.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using Pgvector.EntityFrameworkCore;
using Taskit.AI.Orchestrator.Settings;
using Taskit.Domain.Entities;
using Taskit.Domain.Enums;
using Taskit.Domain.Events;
using Taskit.Infrastructure;

namespace Taskit.AI.Orchestrator.Consumers;

public class DetectDuplicatesConsumer(
    OpenAIClient openAiClient,
    AppDbContext db,
    ILogger<DetectDuplicatesConsumer> logger) : IConsumer<TaskEmbeddingsCreated>
{
    private readonly OpenAIClient _openAiClient = openAiClient;
    private readonly AppDbContext _db = db;
    private readonly ILogger<DetectDuplicatesConsumer> _logger = logger;
    private const string SystemMessage = """
    You are an AI that detects duplicate tasks based on their descriptions and titles.
    Analyze the provided task and related tasks to identify potential duplicates.
    If a duplicate is found, return "Duplicated".
    If no duplicates are found, return "Not Duplicated".
    Do not include any additional text in your response.
    """;

    public async Task Consume(ConsumeContext<TaskEmbeddingsCreated> context)
    {
        var message = context.Message;

        var taskEmbd = await _db.Set<TaskEmbeddings>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.TaskId == message.TaskId, context.CancellationToken);

        if (taskEmbd is null || (taskEmbd.DescriptionEmbedding is null && taskEmbd.TitleEmbedding is null))
        {
            _logger.LogWarning("No embeddings found for task {TaskId}. Cannot detect duplicates.", message.TaskId);
            return;
        }

        var query = taskEmbd.DescriptionEmbedding switch
        {
            not null => _db.Set<TaskEmbeddings>()
                .AsNoTracking()
                .Where(e => e.TaskId != message.TaskId && e.DescriptionEmbedding != null)
                .OrderBy(e => taskEmbd.DescriptionEmbedding!.CosineDistance(e.DescriptionEmbedding!)),
            _ => _db.Set<TaskEmbeddings>()
                .AsNoTracking()
                .Where(e => e.TaskId != message.TaskId && e.TitleEmbedding != null)
                .OrderBy(e => taskEmbd.TitleEmbedding!.CosineDistance(e.TitleEmbedding!))
        };

        var relatedIds = await query
            .Include(e => e.Task)
            .Where(e => e.Task != null && e.Task.ProjectId == message.ProjectId)
            .Select(e => e.TaskId)
            .Take(5) // Limit to 5 related tasks
            .ToListAsync(context.CancellationToken);

        if (relatedIds.Count == 0)
        {
            // No related tasks found, cannot detect duplicates
            return;
        }

        var task = await _db.Tasks
            .AsNoTracking()
            .Where(t => t.Id == message.TaskId)
            .Select(t => new { t.Title, t.Description })
            .FirstOrDefaultAsync(context.CancellationToken)
            ?? throw new InvalidOperationException($"Task with ID {message.TaskId} not found.");

        var relatedTasks = await _db.Tasks
            .AsNoTracking()
            .Where(t => relatedIds.Contains(t.Id))
            .Select(t => new { t.Id, t.Title, t.Description })
            .ToListAsync(context.CancellationToken);

        try
        {
            var client = _openAiClient.GetChatClient("gpt-4.1-mini");

            foreach (var relatedTask in relatedTasks)
            {
                var completion = await client.CompleteChatAsync([
                    new SystemChatMessage(SystemMessage),
                    new UserChatMessage($"Task Title: {task.Title}\nTask Description: {task.Description}\n\nRelated Task Title: {relatedTask.Title}\nRelated Task Description: {relatedTask.Description}")
                ], cancellationToken: context.CancellationToken);

                var response = completion.Value.Content.FirstOrDefault()?.Text?.Trim();
                if (string.IsNullOrWhiteSpace(response))
                {
                    // NOTE: Handle empty response gracefully
                    continue; // Skip to the next related task
                }

                if (response.Equals("Duplicated", StringComparison.OrdinalIgnoreCase))
                {
                    // TODO: Handle the case where a duplicate is detected
                    await context.Publish(new TaskDuplicateDetected(
                        Id: Guid.NewGuid(),
                        UserId: message.UserId,
                        TaskId: message.TaskId,
                        RelatedTaskId: relatedTask.Id,
                        ProjectId: message.ProjectId,
                        Timestamp: DateTime.UtcNow
                    ), context.CancellationToken);
                    return; // Exit after detecting the first duplicate
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to detect duplicates for task {TaskId}", message.TaskId);
        }
    }
}

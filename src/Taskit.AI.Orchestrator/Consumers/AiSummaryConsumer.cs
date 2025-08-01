using System;
using System.Linq;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using Taskit.Domain.Enums;
using Taskit.Domain.Events;
using Taskit.Infrastructure;

namespace Taskit.AI.Orchestrator.Consumers;

public class AiSummaryConsumer(
    OpenAIClient openAiClient,
    AppDbContext db,
    IConfiguration configuration,
    ILogger<AiSummaryConsumer> logger) : IConsumer<ProjectActivityLogCreated>
{
    private readonly OpenAIClient _openAiClient = openAiClient;
    private readonly AppDbContext _db = db;
    private readonly ILogger<AiSummaryConsumer> _logger = logger;
    private readonly string _model = configuration["OpenAI:Model"] ?? "gpt-4o-mini";

    public async Task Consume(ConsumeContext<ProjectActivityLogCreated> context)
    {
        var evt = context.Message;

        if (evt.EventType != ProjectActivityLogEventType.TaskCreated || evt.TaskId is null)
            return;

        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == evt.TaskId, context.CancellationToken);
        if (task is null || string.IsNullOrEmpty(task.Description))
            return;

        try
        {
            var chatClient = _openAiClient.GetChatClient(_model);
            var completion = await chatClient.CompleteChatAsync([
                new SystemChatMessage("You create concise summaries of task descriptions."),
                new UserChatMessage($"Title: {task.Title}\nDescription: {task.Description}")
            ], cancellationToken: context.CancellationToken);

            var summary = completion.Value.Content.FirstOrDefault()?.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(summary))
            {
                task.GeneratedSummary = summary;
                await _db.SaveChangesAsync(context.CancellationToken);
                _logger.LogInformation("Generated summary for task {TaskId}", task.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate summary for task {TaskId}", evt.TaskId);
        }
    }
}

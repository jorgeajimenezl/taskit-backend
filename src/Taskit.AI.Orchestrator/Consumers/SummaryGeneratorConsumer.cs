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
using Taskit.AI.Orchestrator.Settings;
using Taskit.Domain.Enums;
using Taskit.Domain.Events;
using Taskit.Infrastructure;

namespace Taskit.AI.Orchestrator.Consumers;

public class SummaryGeneratorConsumer(
    OpenAIClient openAiClient,
    AppDbContext db,
    IOptions<SummaryGeneratorSettings> settings,
    ILogger<SummaryGeneratorConsumer> logger) : IConsumer<ProjectActivityLogCreated>
{
    private readonly OpenAIClient _openAiClient = openAiClient;
    private readonly AppDbContext _db = db;
    private readonly ILogger<SummaryGeneratorConsumer> _logger = logger;
    private readonly SummaryGeneratorSettings _settings = settings.Value;

    private const string SystemMessage = """
    You are an AI that generates concise summaries of task descriptions.
    Avoid unnecessary details and focus on clarity and brevity.
    The length of the summary should be around one or two short sentences.
    Only generate the summary, do not include any additional text.
    """;

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
            var chatClient = _openAiClient.GetChatClient(_settings.Model);
            var completion = await chatClient.CompleteChatAsync([
                new SystemChatMessage(SystemMessage),
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

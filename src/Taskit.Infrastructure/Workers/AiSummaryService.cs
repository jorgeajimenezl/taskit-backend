using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using Taskit.Domain.Entities;
using Polly;

namespace Taskit.Infrastructure.Workers;

public class AiSummaryService(
    IServiceProvider services,
    ILogger<AiSummaryService> logger,
    IConfiguration configuration,
    OpenAIClient openAiClient) : BackgroundService
{
    private readonly IServiceProvider _services = services;
    private readonly ILogger<AiSummaryService> _logger = logger;
    private readonly OpenAIClient _openAiClient = openAiClient;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(
        configuration.GetValue<int>("AISummaryIntervalMinutes", 5));
    private readonly string _model = configuration["OpenAI:Model"] ?? "gpt-4o-mini";
    private readonly int _batchSize = configuration.GetValue<int>("AISummaryBatchSize", 50);
    private readonly TimeSpan _requestDelay = TimeSpan.FromMilliseconds(
        configuration.GetValue<int>("AISummaryDelayMilliseconds", 1000));
    private readonly AsyncPolicy _retryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(
            configuration.GetValue<int>("AISummaryRetryCount", 3),
            attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
            (exception, delay, attempt, _) =>
                logger.LogWarning(exception,
                    "Retrying summary generation in {Delay} (attempt {Attempt})",
                    delay, attempt));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_interval);
        do
        {
            try
            {
                await GenerateSummariesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during summary generation");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task GenerateSummariesAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var chatClient = _openAiClient.GetChatClient(_model);

        var lastId = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            var batch = await context.Tasks
                .AsNoTracking()
                .Where(t => t.Id > lastId && string.IsNullOrEmpty(t.GeneratedSummary) && !string.IsNullOrEmpty(t.Description))
                .OrderBy(t => t.Id)
                .Take(_batchSize)
                .ToListAsync(cancellationToken);

            if (batch.Count == 0)
                break;

            var tasksToUpdate = new List<AppTask>();

            foreach (var task in batch)
            {
                try
                {
                    var completion = await _retryPolicy.ExecuteAsync(
                        ct => chatClient.CompleteChatAsync([
                            new SystemChatMessage("You create concise summaries of task descriptions."),
                            new UserChatMessage($"Title: {task.Title}\nDescription: {task.Description}")
                        ], cancellationToken: ct),
                        cancellationToken);

                    var summary = completion.Value.Content.FirstOrDefault()?.Text?.Trim();
                    if (!string.IsNullOrWhiteSpace(summary))
                    {
                        task.GeneratedSummary = summary;
                        tasksToUpdate.Add(task);
                        _logger.LogInformation("Generated summary for task {TaskId}", task.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to generate summary for task {TaskId}", task.Id);
                }

                await Task.Delay(_requestDelay, cancellationToken);
            }

            if (tasksToUpdate.Count > 0)
                context.Tasks.UpdateRange(tasksToUpdate);

            await context.SaveChangesAsync(cancellationToken);
            lastId = batch[^1].Id;
        }
    }
}

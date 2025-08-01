using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;

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
                .Where(t => t.Id > lastId && string.IsNullOrEmpty(t.GeneratedSummary) && !string.IsNullOrEmpty(t.Description))
                .OrderBy(t => t.Id)
                .Take(_batchSize)
                .ToListAsync(cancellationToken);

            if (batch.Count == 0)
                break;

            foreach (var task in batch)
            {
                try
                {
                    var completion = await chatClient.CompleteChatAsync([
                        new SystemChatMessage("You create concise summaries of task descriptions."),
                        new UserChatMessage($"Title: {task.Title}\nDescription: {task.Description}")
                    ], cancellationToken: cancellationToken);

                    var summary = completion.Value.Content.FirstOrDefault()?.Text?.Trim();
                    if (!string.IsNullOrWhiteSpace(summary))
                    {
                        task.GeneratedSummary = summary;
                        context.Tasks.Update(task);
                        _logger.LogInformation("Generated summary for task {TaskId}", task.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to generate summary for task {TaskId}", task.Id);
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }

            await context.SaveChangesAsync(cancellationToken);
            lastId = batch[^1].Id;
        }
    }
}

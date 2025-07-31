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

        var tasks = await context.Tasks
            .Where(t => string.IsNullOrEmpty(t.GeneratedSummary) && !string.IsNullOrEmpty(t.Description))
            .ToListAsync(cancellationToken);

        if (tasks.Count == 0)
            return;

        var chatClient = _openAiClient.GetChatClient(_model);

        foreach (var task in tasks)
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
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Taskit.Infrastructure.Workers;

public class MediaCleanupService(
    IServiceProvider services,
    ILogger<MediaCleanupService> logger,
    IConfiguration configuration,
    IWebHostEnvironment environment) : BackgroundService
{
    private readonly IServiceProvider _services = services;
    private readonly ILogger<MediaCleanupService> _logger = logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(
        configuration.GetValue<int>("MediaCleanupIntervalMinutes", 5));
    private readonly IWebHostEnvironment _environment = environment;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_interval);
        do
        {
            try
            {
                await CleanupAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during media cleanup");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Starting media cleanup routine");

        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var threshold = DateTime.UtcNow.Add(-_interval);
        var orphans = await context.Media
            .Where(m => !m.IsDeleted) // Only consider non-deleted media
            .Where(m => m.ModelId == null && m.ModelType == null && m.CreatedAt <= threshold)
            .ToListAsync(cancellationToken);

        if (orphans.Count == 0)
            return;

        var uploadsPath = Path.Combine(
            _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
            "uploads");

        foreach (var media in orphans)
        {
            var sanitizedFileName = Path.GetFileName(media.FileName);
            var path = Path.Combine(uploadsPath, sanitizedFileName);
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                    media.IsDeleted = true;
                    media.DeletedAt = DateTime.UtcNow;
                    context.Media.Update(media);
                }
                catch (IOException ex)
                {
                    _logger.LogWarning(ex, "Failed to delete orphan file {File}", path);
                }

            }
        }

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deleted {Count} orphaned media records", orphans.Count);
    }
}
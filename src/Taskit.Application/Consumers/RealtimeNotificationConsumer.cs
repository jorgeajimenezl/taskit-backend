using MassTransit;
using Microsoft.Extensions.Logging;
using Taskit.Domain.Events;

namespace Taskit.Application.Consumers;

public class RealtimeNotificationConsumer(ILogger<RealtimeNotificationConsumer> logger) : IConsumer<NotificationCreated>
{
    private readonly ILogger<RealtimeNotificationConsumer> _logger = logger;

    public Task Consume(ConsumeContext<NotificationCreated> context)
    {
        _logger.LogInformation("Notification created: {NotificationId}",
            context.Message.NotificationId);
        return Task.CompletedTask;
    }
}
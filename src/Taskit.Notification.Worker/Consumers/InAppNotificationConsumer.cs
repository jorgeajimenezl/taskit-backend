using MassTransit;
using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;
using Taskit.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Taskit.Notification.Worker.Consumers;

public class InAppNotificationConsumer(INotificationRepository notificationRepository, ILogger<InAppNotificationConsumer> logger)
    : IConsumer<ProjectActivityLogCreated>
{
    private readonly INotificationRepository _notificationRepository = notificationRepository;
    private readonly ILogger<InAppNotificationConsumer> _logger = logger;

    public Task Consume(ConsumeContext<ProjectActivityLogCreated> context)
    {
        var _event = context.Message;
        _logger.LogInformation("Received event: {EventType} for UserId: {UserId}", _event.EventType, _event.UserId);
        return Task.CompletedTask;
    }
}
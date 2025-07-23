using MassTransit;
using Taskit.Domain.Events;
using Taskit.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Taskit.Notification.Worker.Consumers;

public class EmailNotificationConsumer(ILogger<EmailNotificationConsumer> logger) : IConsumer<ProjectActivityLogCreated>
{
    private readonly ILogger<EmailNotificationConsumer> _logger = logger;

    public Task Consume(ConsumeContext<ProjectActivityLogCreated> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "Email Notification: {Id}, User: {UserId}, Event: {EventType}, ProjectId: {ProjectId}, TaskId: {TaskId}",
            msg.Id,
            msg.UserId,
            msg.EventType,
            msg.ProjectId,
            msg.TaskId);
        return Task.CompletedTask;
    }
}
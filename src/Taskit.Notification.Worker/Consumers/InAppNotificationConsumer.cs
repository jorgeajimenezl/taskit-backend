using MassTransit;
using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;
using Taskit.Domain.Events;

namespace Taskit.Notification.Worker.Consumers;

public class InAppNotificationConsumer(INotificationRepository notificationRepository)
    : IConsumer<ProjectActivityLogCreated>
{
    private readonly INotificationRepository _notificationRepository = notificationRepository;

    public Task Consume(ConsumeContext<ProjectActivityLogCreated> context)
    {
        var _event = context.Message;
        Console.WriteLine($"Received event: {_event.EventType} for UserId: {_event.UserId}");
        return Task.CompletedTask;
    }
}
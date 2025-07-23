using MassTransit;
using Taskit.Domain.Events;
using Taskit.Domain.Interfaces;

namespace Taskit.Notification.Worker.Consumers;

public class EmailNotificationConsumer() : IConsumer<NotificationCreated>
{
    public Task Consume(ConsumeContext<NotificationCreated> context)
    {
        var msg = context.Message;
        Console.WriteLine($"Notification Created: {msg.NotificationId}, User: {msg.UserId}, Title: {msg.Title}, Type: {msg.Type}");
        return Task.CompletedTask;
    }
}
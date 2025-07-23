using MassTransit;
using Taskit.Domain.Events;
using Taskit.Domain.Interfaces;

namespace Taskit.Notification.Worker.Consumers;

public class EmailNotificationConsumer() : IConsumer<ProjectActivityLogCreated>
{
    public Task Consume(ConsumeContext<ProjectActivityLogCreated> context)
    {
        var msg = context.Message;
        Console.WriteLine($"Email Notification: {msg.Id}, User: {msg.UserId}, Event: {msg.EventType}, ProjectId: {msg.ProjectId}, TaskId: {msg.TaskId}");
        return Task.CompletedTask;
    }
}
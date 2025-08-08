using MassTransit;
using Taskit.Domain.Events;

namespace Taskit.Notification.Worker.Consumers;

public class RealtimeNotificationConsumer() : IConsumer<NotificationCreated>
{
    public Task Consume(ConsumeContext<NotificationCreated> context)
    {
        throw new NotImplementedException();
    }
}
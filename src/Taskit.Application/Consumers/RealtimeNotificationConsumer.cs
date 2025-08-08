using MassTransit;
using Taskit.Domain.Events;

namespace Taskit.Application.Consumers;

public class RealtimeNotificationConsumer() : IConsumer<NotificationCreated>
{
    public Task Consume(ConsumeContext<NotificationCreated> context)
    {
        throw new NotImplementedException();
    }
}
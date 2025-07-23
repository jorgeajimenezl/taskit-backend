using MassTransit;
using Taskit.Domain.Events;
using Taskit.Domain.Interfaces;

namespace Taskit.Notification.Worker;

public class EmailNotificationConsumer(IEmailSender emailSender) : IConsumer<NotificationCreated>
{
    public async Task Consume(ConsumeContext<NotificationCreated> context)
    {
        var msg = context.Message;
        await emailSender.SendAsync(msg.UserId, msg.Title, msg.Message);
    }
}

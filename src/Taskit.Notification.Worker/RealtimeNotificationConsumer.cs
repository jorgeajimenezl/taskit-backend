using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Taskit.Domain.Entities;
using Taskit.Domain.Events;
using Taskit.Infrastructure;

namespace Taskit.Notification.Worker;

public class RealtimeNotificationConsumer(AppDbContext db, IHubContext<NotificationHub> hub)
    : IConsumer<NotificationCreated>
{
    public async Task Consume(ConsumeContext<NotificationCreated> context)
    {
        var msg = context.Message;
        var exists = await db.Notifications.AnyAsync(n =>
            n.UserId == msg.UserId &&
            n.Title == msg.Title &&
            n.Message == msg.Message &&
            n.Type == msg.Type);

        if (!exists)
        {
            var notification = new Taskit.Domain.Entities.Notification
            {
                Title = msg.Title,
                Message = msg.Message,
                Type = msg.Type,
                Data = msg.Data,
                UserId = msg.UserId
            };
            await db.Notifications.AddAsync(notification);
            await db.SaveChangesAsync();
        }

        await hub.Clients.Group($"user:{msg.UserId}")
            .SendAsync("ReceiveNotification", msg);
    }
}

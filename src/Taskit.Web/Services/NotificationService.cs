using MassTransit;
using Taskit.Domain.Entities;
using Taskit.Domain.Enums;
using Taskit.Domain.Events;
using Taskit.Infrastructure;

namespace Taskit.Web.Services;

public class NotificationService(AppDbContext context, IPublishEndpoint publisher)
{
    private readonly AppDbContext _context = context;
    private readonly IPublishEndpoint _publisher = publisher;

    public async Task CreateAsync(string userId, string title, NotificationType type, string? message = null, IDictionary<string, object?>? data = null)
    {
        var notification = new Notification
        {
            Title = title,
            Message = message,
            Type = type,
            Data = data,
            UserId = userId
        };

        await _context.Notifications.AddAsync(notification);
        await _publisher.Publish(new NotificationCreated(userId, title, message, type, data));
        await _context.SaveChangesAsync();
    }
}

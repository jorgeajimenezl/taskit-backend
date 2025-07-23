using MassTransit;
using Taskit.Domain.Events;
using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;
using Taskit.Domain.Enums;

namespace Taskit.Application.Services;

public class NotificationService(INotificationRepository notificationRepository, IPublishEndpoint publisher)
{
    private readonly INotificationRepository _notificationRepository = notificationRepository;
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

        await _notificationRepository.AddAsync(notification);
    }
}
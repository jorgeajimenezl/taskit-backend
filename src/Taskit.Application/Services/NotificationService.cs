using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;
using Taskit.Domain.Enums;

namespace Taskit.Application.Services;

public class NotificationService(INotificationRepository notificationRepository)
{
    private readonly INotificationRepository _notificationRepository = notificationRepository;

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
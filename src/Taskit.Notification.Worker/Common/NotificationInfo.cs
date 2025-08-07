using Taskit.Domain.Enums;

namespace Taskit.Notification.Worker.Common;

public record NotificationInfo(
    string Title,
    string Message,
    NotificationType Type
);
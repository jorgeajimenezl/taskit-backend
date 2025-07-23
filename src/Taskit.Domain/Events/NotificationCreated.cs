using Taskit.Domain.Enums;

namespace Taskit.Domain.Events;

public record NotificationCreated
(
    int NotificationId,
    string UserId,
    string Title,
    string? Message,
    NotificationType Type,
    IDictionary<string, object?>? Data
);
namespace Taskit.Domain.Events;

public record NotificationCreated(
    Guid Id,
    int NotificationId,
    DateTime Timestamp
) : IEvent<NotificationCreated>;
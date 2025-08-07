using MimeKit;
using Taskit.Domain.Events;
using Taskit.Notification.Worker.Common;

namespace Taskit.Notification.Worker.Interfaces;

public interface IInAppMessageGenerator<TEvent> where TEvent : class, IEvent<TEvent>
{
    Task<NotificationInfo> GenerateAsync(TEvent @event, string recipientId, CancellationToken cancellationToken = default);
}

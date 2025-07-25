using MimeKit;
using Taskit.Domain.Events;

namespace Taskit.Notification.Worker.Interfaces;

public interface IEmailMessageGenerator<TEvent> where TEvent : class, IEvent<TEvent>
{
    Task<MimeMessage> GenerateAsync(TEvent @event, string recipientEmail, CancellationToken cancellationToken = default);
}

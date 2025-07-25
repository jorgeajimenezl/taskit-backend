using MimeKit;
using Taskit.Domain.Events;

namespace Taskit.Notification.Worker.Services;

public interface IEmailMessageGenerator
{
    Task<MimeMessage> GenerateAsync(ProjectActivityLogCreated @event, string recipientEmail, CancellationToken cancellationToken = default);
}

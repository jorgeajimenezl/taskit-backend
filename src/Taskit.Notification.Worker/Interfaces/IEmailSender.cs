using MimeKit;

namespace Taskit.Notification.Worker.Interfaces;

public interface IEmailSender
{
    Task SendAsync(MimeMessage message, CancellationToken cancellationToken = default);
}

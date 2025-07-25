using MimeKit;

namespace Taskit.Notification.Worker.Services;

public interface IEmailSender
{
    Task SendAsync(MimeMessage message, CancellationToken cancellationToken = default);
}

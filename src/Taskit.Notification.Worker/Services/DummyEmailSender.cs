using MimeKit;
using Taskit.Domain.Entities;
using Taskit.Notification.Worker.Interfaces;

namespace Taskit.Notification.Worker.Services;

public class DummyEmailSender(ILogger<DummyEmailSender> logger) : IEmailSender
{
    private readonly ILogger<DummyEmailSender> _logger = logger;

    public Task SendAsync(MimeMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Dummy email sender invoked. Message [{}]: {Subject}",
            message.To.FirstOrDefault(),
            message.Subject
        );
        return Task.CompletedTask;
    }
}

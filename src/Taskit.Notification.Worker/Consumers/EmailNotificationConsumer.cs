using MassTransit;
using Taskit.Domain.Events;
using Taskit.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Taskit.Notification.Worker.Services;

namespace Taskit.Notification.Worker.Consumers;

public class EmailNotificationConsumer(
    IRecipientResolver recipientResolver,
    IEmailSender emailSender,
    IEmailMessageGenerator messageGenerator,
    ILogger<EmailNotificationConsumer> logger) : IConsumer<ProjectActivityLogCreated>
{
    private readonly ILogger<EmailNotificationConsumer> _logger = logger;
    private readonly IRecipientResolver _recipientResolver = recipientResolver;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly IEmailMessageGenerator _messageGenerator = messageGenerator;

    public async Task Consume(ConsumeContext<ProjectActivityLogCreated> context)
    {
        var evt = context.Message;
        var recipients = await _recipientResolver.GetRecipientsAsync(evt, context.CancellationToken);

        foreach (var email in recipients)
        {
            var message = await _messageGenerator.GenerateAsync(evt, email, context.CancellationToken);
            await _emailSender.SendAsync(message, context.CancellationToken);
        }

        _logger.LogInformation("Processed email notification for event {Id}", evt.Id);
    }

}

using MassTransit;
using Taskit.Domain.Events;
using Taskit.Notification.Worker.Interfaces;

namespace Taskit.Notification.Worker.Consumers;

public class EmailNotificationConsumer<TEvent>(
    IRecipientResolver<TEvent> recipientResolver,
    IEmailSender emailSender,
    IEmailMessageGenerator<TEvent> messageGenerator,
    ILogger<EmailNotificationConsumer<TEvent>> logger) : IConsumer<TEvent>
    where TEvent : class, IEvent<TEvent>
{
    private readonly ILogger<EmailNotificationConsumer<TEvent>> _logger = logger;
    private readonly IRecipientResolver<TEvent> _recipientResolver = recipientResolver;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly IEmailMessageGenerator<TEvent> _messageGenerator = messageGenerator;

    public async Task Consume(ConsumeContext<TEvent> context)
    {
        var evt = context.Message;
        var recipients = await _recipientResolver.GetRecipientsAsync(evt, context.CancellationToken);

        foreach (var recipientUser in recipients)
        {
            var email = recipientUser.Email;
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("User {UserId} has no email address, skipping notification for event {EventId}", recipientUser.Id, evt.Id);
                continue;
            }
            var message = await _messageGenerator.GenerateAsync(evt, email, context.CancellationToken);
            await _emailSender.SendAsync(message, context.CancellationToken);
        }

        _logger.LogInformation("Processed email notification for event {Id}", evt.Id);
    }

}

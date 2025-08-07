using MassTransit;
using Taskit.Application.Interfaces;
using Taskit.Domain.Events;
using Taskit.Notification.Worker.Interfaces;

namespace Taskit.Notification.Worker.Consumers;

public class InAppNotificationConsumer<TEvent>(
    IRecipientResolver<TEvent> recipientResolver,
    IInAppMessageGenerator<TEvent> messageGenerator,
    INotificationRepository notificationRepository,
    ILogger<InAppNotificationConsumer<TEvent>> logger) : IConsumer<TEvent>
    where TEvent : class, IEvent<TEvent>
{
    private readonly ILogger<InAppNotificationConsumer<TEvent>> _logger = logger;
    private readonly IRecipientResolver<TEvent> _recipientResolver = recipientResolver;
    private readonly IInAppMessageGenerator<TEvent> _messageGenerator = messageGenerator;
    private readonly INotificationRepository _notificationRepository = notificationRepository;

    public async Task Consume(ConsumeContext<TEvent> context)
    {
        var evt = context.Message;
        var recipients = await _recipientResolver.GetRecipientsAsync(evt, context.CancellationToken);

        if (!recipients.Any())
        {
            _logger.LogDebug("No recipients found for event {EventType}", typeof(TEvent).Name);
            return;
        }

        var notifications = new List<Domain.Entities.Notification>();
        foreach (var recipientUser in recipients)
        {
            try
            {
                var message = await _messageGenerator.GenerateAsync(evt, recipientUser.Id, context.CancellationToken);
                notifications.Add(new Domain.Entities.Notification()
                {
                    UserId = recipientUser.Id,
                    Title = message.Title,
                    Message = message.Message,
                    IsRead = false,
                    Type = message.Type,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate notification for user {UserId}", recipientUser.Id);
            }
        }

        if (notifications.Any())
        {
            await _notificationRepository.AddRangeAsync(notifications, cancellationToken: context.CancellationToken);
            _logger.LogInformation("Created {Count} notifications for event {EventType}", notifications.Count, typeof(TEvent).Name);
        }
    }
}

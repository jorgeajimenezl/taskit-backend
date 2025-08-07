using Microsoft.Extensions.Options;
using MimeKit;
using Taskit.Domain.Enums;
using Taskit.Domain.Events;
using Taskit.Notification.Worker.Common;
using Taskit.Notification.Worker.Interfaces;
using Taskit.Notification.Worker.Settings;

namespace Taskit.Notification.Worker.Services.MessageGenerators.Email;

public class TaskDuplicateInAppMessageGenerator() : IInAppMessageGenerator<TaskDuplicateDetected>
{
    public Task<NotificationInfo> GenerateAsync(TaskDuplicateDetected @event, string recipientId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

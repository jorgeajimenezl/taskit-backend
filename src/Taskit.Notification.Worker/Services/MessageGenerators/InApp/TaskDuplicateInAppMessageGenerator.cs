using Taskit.Domain.Events;
using Taskit.Notification.Worker.Common;
using Taskit.Notification.Worker.Interfaces;

namespace Taskit.Notification.Worker.Services.MessageGenerators.Email;

public class TaskDuplicateInAppMessageGenerator() : IInAppMessageGenerator<TaskDuplicateDetected>
{
    public Task<NotificationInfo> GenerateAsync(TaskDuplicateDetected @event, string recipientId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

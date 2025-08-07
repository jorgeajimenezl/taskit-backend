using Taskit.Domain.Enums;
using Taskit.Domain.Events;
using Taskit.Notification.Worker.Common;
using Taskit.Notification.Worker.Interfaces;

namespace Taskit.Notification.Worker.Services.MessageGenerators.Email;

public class TaskDuplicateInAppMessageGenerator() : IInAppMessageGenerator<TaskDuplicateDetected>
{
    public Task<NotificationInfo> GenerateAsync(
        TaskDuplicateDetected @event,
        string recipientId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new NotificationInfo(
            "Task Duplicate Detected",
            $"The recent task with ID {@event.TaskId} has been detected as a duplicate of the task with ID {@event.RelatedTaskId}.",
            NotificationType.Warning
        ));
    }
}

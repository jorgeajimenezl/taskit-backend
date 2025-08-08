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
        var taskLink = $"/tasks/{@event.TaskId}";
        var relatedTaskLink = $"/tasks/{@event.RelatedTaskId}";

        return Task.FromResult(new NotificationInfo(
            "Task Duplicate Detected",
            $"The recent <a href=\"{taskLink}\">task</a> has been detected as a duplicate of the already created <a href=\"{relatedTaskLink}\">task</a>.",
            NotificationType.Warning
        ));
    }
}

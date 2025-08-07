using Taskit.Domain.Events;

namespace Taskit.Domain.Events;

public record TaskDuplicateDetected(
    Guid Id,
    string UserId,
    int TaskId,
    int RelatedTaskId,
    int ProjectId,
    DateTime Timestamp
) : IEvent<TaskDuplicateDetected>;
using Taskit.Domain.Events;

namespace Taskit.Domain.Events;

public record TaskEmbeddingsCreated(
    Guid Id,
    string UserId,
    int TaskId,
    int ProjectId,
    DateTime Timestamp = default
) : IEvent<TaskEmbeddingsCreated>;
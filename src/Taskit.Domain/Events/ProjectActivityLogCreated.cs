using Taskit.Domain.Enums;

namespace Taskit.Domain.Events;

public record ProjectActivityLogCreated(
    int Id,
    ProjectActivityLogEventType EventType,
    string UserId,
    int? ProjectId = null,
    int? TaskId = null,
    IDictionary<string, object?>? Data = null,
    DateTime Timestamp = default);
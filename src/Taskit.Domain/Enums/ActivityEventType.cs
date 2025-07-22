namespace Taskit.Domain.Enums;

public enum ActivityEventType
{
    TaskCreated,
    TaskAssigned,
    TaskStatusChanged,
    TaskCompleted,
    CommentAdded,
    UserJoinedProject,
    UserLeftProject,
    FileAttached
}

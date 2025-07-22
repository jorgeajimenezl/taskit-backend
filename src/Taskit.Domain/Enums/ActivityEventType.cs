namespace Taskit.Domain.Enums;

public enum ActivityEventType
{
    TaskCreated,
    TaskAssigned,
    TaskUpdated,
    TaskDeleted,
    TaskStatusChanged,
    CommentAdded,
    UserJoinedProject,
    UserLeftProject,
    FileAttached,
    FileUploaded,
    FileDeleted,
    ProjectCreated,
    ProjectUpdated,
    ProjectDeleted,
}

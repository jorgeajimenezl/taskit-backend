namespace Taskit.Domain.Enums;

public enum ActivityEventType
{
    TaskCreated,
    TaskAssigned,
    TaskStatusChanged,
    CommentAdded,
    UserJoinedProject,
    UserLeftProject,
    FileAttached,
    FileUploaded,
    FileDeleted,
    ProjectCreated,
    ProjectUpdated,
}

namespace Taskit.Domain.Enums;

public enum TaskStatus
{
    /// <summary>
    /// Task has been created but not yet started.
    /// </summary>
    Created = 0,

    /// <summary>
    /// Task has been selected for processing.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Task is currently being worked on.
    /// </summary>
    InProgress = 2,

    /// <summary>
    /// Task has been completed successfully.
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Task is overdue and requires attention.
    /// </summary>
    Overdue = 4,

    /// <summary>
    /// Task has been cancelled and will not be processed.
    /// </summary>
    Cancelled = 5
}
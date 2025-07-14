namespace Taskit.Domain.Entities;

public class TaskComment : BaseEntity
{
    public required string Content { get; set; }

    public required int TaskId { get; set; }
    public required AppTask Task { get; set; }

    public required string AuthorId { get; set; }
    public required AppUser Author { get; set; }
}
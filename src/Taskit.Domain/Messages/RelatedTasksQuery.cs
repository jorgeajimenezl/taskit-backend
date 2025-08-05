namespace Taskit.Domain.Messages;

public record RelatedTasksQuery
{
    public int TaskId { get; init; }
    public int Count { get; init; } = 5;
}

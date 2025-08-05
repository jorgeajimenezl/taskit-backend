namespace Taskit.Domain.Messages;

public record RelatedTasksQuery(int TaskId, int Count = 5);

namespace Taskit.Domain.Messages;

public record RelatedTasksQueryResult(IReadOnlyCollection<int> TaskIds);
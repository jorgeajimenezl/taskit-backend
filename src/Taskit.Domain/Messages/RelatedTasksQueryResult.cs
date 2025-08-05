namespace Taskit.Domain.Messages;

public record RelatedTasksQueryResult(bool IsProcessing, IReadOnlyCollection<int>? TaskIds);

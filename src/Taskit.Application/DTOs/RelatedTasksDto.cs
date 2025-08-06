namespace Taskit.Application.DTOs;

public record RelatedTasksDto
{
    public bool IsProcessing { get; init; }
    public IEnumerable<TaskDto> Tasks { get; init; } = [];
}

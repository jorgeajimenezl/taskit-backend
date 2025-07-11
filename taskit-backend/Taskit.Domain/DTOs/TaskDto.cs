namespace Taskit.Domain.DTOs;

public record TaskDto
{
    public int Id { get; init; }
    public string Title { get; init; }
    public string Description { get; init; }
    public DateTime? DueDate { get; init; }
    public DateTime? CompletedAt { get; init; }
    public Enums.TaskStatus Status { get; init; }
}
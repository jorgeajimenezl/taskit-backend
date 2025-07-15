using System.ComponentModel.DataAnnotations;
using Taskit.Domain.Enums;

namespace Taskit.Application.DTOs;

public record CreateTaskRequest
{
    [Required]
    [StringLength(100)]
    public required string Title { get; init; }

    public string? Description { get; init; }

    public DateTime? DueDate { get; init; }

    public TaskStatus Status { get; init; } = TaskStatus.Created;

    public int? ProjectId { get; init; }

    public string? AssignedUserId { get; init; }
}

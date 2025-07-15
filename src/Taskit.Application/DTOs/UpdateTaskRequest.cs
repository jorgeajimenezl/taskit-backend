using System.ComponentModel.DataAnnotations;
using Taskit.Domain.Enums;

namespace Taskit.Application.DTOs;

public record UpdateTaskRequest
{
    [StringLength(100)]
    public string? Title { get; init; }

    public string? Description { get; init; }

    public DateTime? DueDate { get; init; }

    public TaskStatus? Status { get; init; }

    public string? AssignedUserId { get; init; }

    public bool? IsArchived { get; init; }
}

using System.ComponentModel.DataAnnotations;

namespace Taskit.Application.DTOs;

public record CreateProjectRequest
{
    [Required]
    [StringLength(100)]
    public required string Name { get; init; }

    [StringLength(500)]
    public string? Description { get; init; }
}

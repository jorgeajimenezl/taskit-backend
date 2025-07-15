using System.ComponentModel.DataAnnotations;

namespace Taskit.Application.DTOs;

public record UpdateProjectRequest
{
    [StringLength(100)]
    public string? Name { get; init; }

    [StringLength(500)]
    public string? Description { get; init; }
}

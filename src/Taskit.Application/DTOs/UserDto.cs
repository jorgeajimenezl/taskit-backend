using System.ComponentModel.DataAnnotations;

namespace Taskit.Application.DTOs;

public record UserDto
{
    [Required]
    public required string Id { get; init; }

    [Required]
    public required string UserName { get; init; }

    [Required]
    public required string Email { get; init; }

    [Required]
    public required string FullName { get; init; }
}
using System.ComponentModel.DataAnnotations;

namespace Taskit.Application.DTOs;

public record LoginRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public required string Password { get; init; }
}

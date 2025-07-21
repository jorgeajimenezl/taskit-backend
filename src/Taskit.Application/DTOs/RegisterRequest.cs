using System.ComponentModel.DataAnnotations;

namespace Taskit.Application.DTOs;

public record RegisterRequest
{
    [StringLength(100)]
    public string? FullName { get; init; }

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public required string Username { get; init; }

    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public required string Password { get; init; }
}

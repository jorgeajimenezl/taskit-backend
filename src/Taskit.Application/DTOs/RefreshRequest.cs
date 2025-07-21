using System.ComponentModel.DataAnnotations;

namespace Taskit.Application.DTOs;

public record RefreshRequest
{
    [Required]
    public required string RefreshToken { get; init; }
}

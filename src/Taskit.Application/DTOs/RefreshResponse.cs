using System.ComponentModel.DataAnnotations;

namespace Taskit.Application.DTOs;

public record RefreshResponse
{
    [Required]
    public required string AccessToken { get; init; }

    public string? RefreshToken { get; init; }
}


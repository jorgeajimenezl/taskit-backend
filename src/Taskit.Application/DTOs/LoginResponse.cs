using System.ComponentModel.DataAnnotations;

namespace Taskit.Application.DTOs;

public record LoginResponse
{
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }

    [Required]
    public required UserProfileDto User { get; init; }
}


using System.ComponentModel.DataAnnotations;

namespace Taskit.Application.DTOs;

public record LoginResponse
{
    [Required]
    public required string AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public UserDto? User { get; init; }
}


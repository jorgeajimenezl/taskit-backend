namespace Taskit.Application.DTOs;

public record LoginResponse
{
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public UserProfileDto? User { get; init; }
}


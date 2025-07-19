namespace Taskit.Application.DTOs;

public record LoginResponse
{
    public required string AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public AppUserDto? User { get; init; }
}


namespace Taskit.Application.DTOs;

public record RefreshResponse
{
    public required string AccessToken { get; init; }
    public string? RefreshToken { get; init; }
}


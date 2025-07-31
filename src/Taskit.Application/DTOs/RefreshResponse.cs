namespace Taskit.Application.DTOs;

public record RefreshResponse
{
    public string? AccessToken { get; init; }

    public string? RefreshToken { get; init; }
}


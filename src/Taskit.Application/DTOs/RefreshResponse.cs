namespace Taskit.Application.DTOs;

public record RefreshResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
}

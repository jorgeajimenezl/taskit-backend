namespace Taskit.Application.DTOs;

public record RefreshResponse
{
    public required string Token { get; init; }
    public required string RefreshToken { get; init; }
}

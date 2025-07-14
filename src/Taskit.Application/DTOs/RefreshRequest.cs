namespace Taskit.Application.DTOs;

public record RefreshRequest
{
    public required string RefreshToken { get; init; }
}

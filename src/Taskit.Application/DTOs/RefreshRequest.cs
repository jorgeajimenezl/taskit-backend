namespace Taskit.Application.DTOs;

public record RefreshRequest
{
    public string? RefreshToken { get; init; }
}

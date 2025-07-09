namespace Taskit.DTOs;

public record LoginResponse
{
    public required string Token { get; init; }
}
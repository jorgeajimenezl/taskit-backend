namespace Taskit.Domain.DTOs;

public record LoginResponse
{
    public required string Token { get; init; }
}
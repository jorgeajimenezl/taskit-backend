namespace Taskit.Domain.DTOs;

public record RegisterRequest
{
    public string? FullName { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
}

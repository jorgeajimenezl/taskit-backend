namespace Taskit.Application.DTOs;

public record ProfileResponse
{
    public string? FullName { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
}
namespace Taskit.Application.DTOs;

public record UpdateProfileRequest
{
    public string? FullName { get; init; }
    public string? Username { get; init; }
    public string? Email { get; init; }
}

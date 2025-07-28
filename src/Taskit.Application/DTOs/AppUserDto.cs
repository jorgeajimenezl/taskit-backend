namespace Taskit.Application.DTOs;

public record AppUserDto
{
    public required string UserName { get; init; }
    public required string Email { get; init; }
    public required string FullName { get; init; }
}
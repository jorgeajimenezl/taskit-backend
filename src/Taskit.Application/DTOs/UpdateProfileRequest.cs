namespace Taskit.Application.DTOs;

using System.ComponentModel.DataAnnotations;

public record UpdateProfileRequest
{
    [StringLength(100)]
    public string? FullName { get; init; }

    [StringLength(100)]
    public string? Username { get; init; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; init; }
}
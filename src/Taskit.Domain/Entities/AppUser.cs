using Microsoft.AspNetCore.Identity;

namespace Taskit.Domain.Entities;

public class AppUser : IdentityUser
{
    public string? FullName { get; set; }
    public List<RefreshToken> RefreshTokens { get; set; } = new();
}
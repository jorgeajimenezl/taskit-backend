using Microsoft.AspNetCore.Identity;

namespace Taskit.Domain.Entities;

public class AppUser : IdentityUser
{
    public string? FullName { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
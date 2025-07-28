using Microsoft.AspNetCore.Identity;

namespace Taskit.Domain.Entities;

public class AppUser : IdentityUser
{
    public string? FullName { get; set; }

    public int? AvatarId { get; set; }
    public Media? Avatar { get; set; }

    // Navigation properties
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
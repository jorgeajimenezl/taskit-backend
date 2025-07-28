using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Taskit.Domain.Entities;

public class AppUser : IdentityUser
{
    public string? FullName { get; set; }

    [ForeignKey(nameof(Avatar))]
    public int? AvatarId { get; set; }
    public UserAvatar? Avatar { get; set; }

    // Navigation properties
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace Taskit.Domain.Entities;

public class RefreshToken : BaseEntity<Guid>
{
    [Required, MaxLength(100)]
    public required string TokenHash { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    [MaxLength(200)]
    public string? UserAgent { get; set; }

    public IPAddress? IpAddress { get; set; }

    [Required, ForeignKey(nameof(User))]
    public required string UserId { get; set; }
    public AppUser? User { get; set; }
}

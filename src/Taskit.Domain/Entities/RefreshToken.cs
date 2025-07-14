using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Taskit.Domain.Entities;

public class RefreshToken
{
    [Key]
    public Guid Id { get; set; }
    public required string TokenHash { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? UserAgent { get; set; }
    public IPAddress? IpAddress { get; set; }

    public required string UserId { get; set; }
    public AppUser? User { get; set; }
}

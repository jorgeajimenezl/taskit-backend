using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Taskit.Domain.Entities;

public class ExternalLogin : BaseEntity<Guid>
{
    [Required, MaxLength(50)]
    public required string Provider { get; set; }

    [Required, MaxLength(250)]
    public required string ProviderUserId { get; set; }

    [Required, ForeignKey(nameof(User))]
    public required string UserId { get; set; }
    public AppUser? User { get; set; }
}

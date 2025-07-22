using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Taskit.Domain.Enums;

namespace Taskit.Domain.Entities
{
    public class Notification : BaseEntity<int>
    {
        [Required, MaxLength(100)]
        public required string Title { get; set; }

        [MaxLength(500)]
        public string? Message { get; set; }

        public NotificationType Type { get; set; }
        public bool IsRead { get; set; } = false;
        public IDictionary<string, object?>? Data { get; set; }

        [Required, ForeignKey(nameof(User))]
        public required string UserId { get; set; }
        public AppUser? User { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace Taskit.Domain.Entities;

public abstract class BaseEntity
{
    [Key]
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public void UpdateTimestamps()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
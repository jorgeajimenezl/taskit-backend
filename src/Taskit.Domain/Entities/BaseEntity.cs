using System.ComponentModel.DataAnnotations;
using Taskit.Domain.Interfaces;

namespace Taskit.Domain.Entities;

public abstract class BaseEntity<TKey> : IEntity<TKey>
{
    [Key]
    public TKey Id { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public void UpdateTimestamps()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
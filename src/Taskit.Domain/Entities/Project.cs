using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Taskit.Domain.Entities;

public class Project : BaseEntity<int>
{
    [Required, MaxLength(100)]
    public required string Name { get; set; }

    [Required, MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required, ForeignKey(nameof(Owner))]
    public required string OwnerId { get; set; }
    public AppUser Owner { get; set; } = null!;

    // Navigation properties
    public ICollection<AppTask> Tasks { get; set; } = [];
    public ICollection<ProjectMember> Members { get; set; } = [];
}
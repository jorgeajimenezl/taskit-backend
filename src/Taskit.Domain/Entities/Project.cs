using System.ComponentModel.DataAnnotations;

namespace Taskit.Domain.Entities;

public class Project : BaseEntity<int>
{
    public required string Name { get; set; }
    public required string Description { get; set; } = string.Empty;

    public required string OwnerId { get; set; }
    public AppUser Owner { get; set; } = null!;

    // Navigation properties
    public ICollection<AppTask> Tasks { get; set; } = [];
    public ICollection<ProjectMember> Members { get; set; } = [];
}
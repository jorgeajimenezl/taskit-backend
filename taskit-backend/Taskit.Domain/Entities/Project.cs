using System.ComponentModel.DataAnnotations;

namespace Taskit.Domain.Entities;

public class Project : BaseEntity
{
    [Required]
    public required string Name { get; set; }
    public required string Description { get; set; } = string.Empty;
    [Required]
    public required string OwnerId { get; set; }
    public AppUser Owner { get; set; } = null!;
    
    // Navigation property for tasks in the project
    public ICollection<AppTask> Tasks { get; set; } = new List<AppTask>();
}
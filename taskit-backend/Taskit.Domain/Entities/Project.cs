namespace Taskit.Domain.Entities;

public class Project : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public AppUser Owner { get; set; } = null!;
    
    // Navigation property for tasks in the project
    public ICollection<AppTask> Tasks { get; set; } = new List<AppTask>();
}
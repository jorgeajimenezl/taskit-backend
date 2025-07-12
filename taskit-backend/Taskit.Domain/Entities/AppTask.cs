using System.ComponentModel.DataAnnotations;
using Taskit.Domain.Enums;

namespace Taskit.Domain.Entities;

public class AppTask : BaseEntity
{
    [Required]
    public required string Title { get; set; }
    public required string Description { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.Created;
    public string? AuthorId { get; set; }
    public AppUser? Author { get; set; }
    public string? AssignedUserId { get; set; }
    public AppUser? AssignedUser { get; set; }
    public int? ProjectId { get; set; }
    public Project? Project { get; set; }
    public int? ParentTaskId { get; set; }
    public AppTask? ParentTask { get; set; }
    public ICollection<AppTask> SubTasks { get; set; } = new List<AppTask>();
}
using System.ComponentModel.DataAnnotations;
using Taskit.Domain.Enums;

namespace Taskit.Domain.Entities;

public class AppTask : BaseEntity
{
    [Required]
    public required string Title { get; set; }
    public required string Description { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; } = null;
    public DateTime? CompletedAt { get; set; } = null;
    public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.Created;
    public string? UserId { get; set; } = null;
    public AppUser User { get; set; } = null!;
    public string? AssignedToId { get; set; } = null;
    public AppUser? AssignedTo { get; set; } = null;
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public int? ParentTaskId { get; set; } = null;
    public AppTask? ParentTask { get; set; } = null;
    public ICollection<AppTask> SubTasks { get; set; } = new List<AppTask>();
}
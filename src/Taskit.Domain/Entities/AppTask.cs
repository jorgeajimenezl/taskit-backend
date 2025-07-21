using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskStatus = Taskit.Domain.Enums.TaskStatus;

namespace Taskit.Domain.Entities;

public class AppTask : BaseEntity<int>
{
    [Required, MaxLength(200)]
    public required string Title { get; set; }

    [Required, MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }

    public TaskStatus Status { get; set; } = TaskStatus.Created;
    public int Priority { get; set; } = 0;
    public bool IsArchived { get; set; } = false;
    public int Complexity { get; set; } = 0;

    [Range(0, 100)]
    public int CompletedPercentage { get; set; } = 0;

    [ForeignKey(nameof(Author))]
    public string? AuthorId { get; set; }
    public AppUser? Author { get; set; }

    [ForeignKey(nameof(AssignedUser))]
    public string? AssignedUserId { get; set; }
    public AppUser? AssignedUser { get; set; }

    [ForeignKey(nameof(Project))]
    public int? ProjectId { get; set; }
    public Project? Project { get; set; }

    [ForeignKey(nameof(ParentTask))]
    public int? ParentTaskId { get; set; }
    public AppTask? ParentTask { get; set; }

    // Navigaiton properties
    public ICollection<AppTask> SubTasks { get; set; } = [];
    public ICollection<TaskTag> Tags { get; set; } = [];
    public ICollection<TaskComment> Comments { get; set; } = [];
}
using System.ComponentModel.DataAnnotations;

namespace Taskit.Models;

public class AppTask : BaseModel
{
    [Required]
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; } = null;
    public DateTime? CompletedAt { get; set; } = null;
    public bool IsCompleted => CompletedAt.HasValue;
    public string? UserId { get; set; } = null;
    public ApplicationUser User { get; set; } = null!;
    public int? ParentTaskId { get; set; } = null;
    public AppTask? ParentTask { get; set; } = null;
    public ICollection<AppTask> SubTasks { get; set; } = new List<AppTask>();
}
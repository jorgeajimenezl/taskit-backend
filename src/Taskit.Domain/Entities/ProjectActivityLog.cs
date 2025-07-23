using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Taskit.Domain.Enums;

namespace Taskit.Domain.Entities;

public class ProjectActivityLog : BaseEntity<int>
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public ProjectActivityLogEventType EventType { get; set; }

    [ForeignKey(nameof(User))]
    public string? UserId { get; set; }
    public AppUser? User { get; set; }

    [ForeignKey(nameof(Project))]
    public int? ProjectId { get; set; }
    public Project? Project { get; set; }

    [ForeignKey(nameof(Task))]
    public int? TaskId { get; set; }
    public AppTask? Task { get; set; }

    public IDictionary<string, object?>? Data { get; set; }
}

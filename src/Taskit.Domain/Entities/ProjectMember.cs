using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Taskit.Domain.Enums;

namespace Taskit.Domain.Entities;

public class ProjectMember : BaseEntity<int>
{
    [Required, ForeignKey(nameof(Project))]
    public required int ProjectId { get; set; }
    public Project? Project { get; set; }

    [Required, ForeignKey(nameof(User))]
    public required string UserId { get; set; }
    public AppUser? User { get; set; }

    [Required]
    public required ProjectRole Role { get; set; } = ProjectRole.Member;
}
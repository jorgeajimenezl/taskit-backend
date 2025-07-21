using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Taskit.Domain.Entities;

public class ProjectMember : BaseEntity<int>
{
    [Required, ForeignKey(nameof(Project))]
    public required int ProjectId { get; set; }
    public required Project Project { get; set; }

    [Required, ForeignKey(nameof(User))]
    public required string UserId { get; set; }
    public required AppUser User { get; set; }

    [Required]
    public required Enums.ProjectRole Role { get; set; } = Enums.ProjectRole.Member;
}
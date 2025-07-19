namespace Taskit.Domain.Entities;

public class ProjectMember : BaseEntity<int>
{
    public required int ProjectId { get; set; }
    public required Project Project { get; set; }
    public required string UserId { get; set; }
    public required AppUser User { get; set; }
    public required Enums.ProjectRole Role { get; set; } = Enums.ProjectRole.Member;
}
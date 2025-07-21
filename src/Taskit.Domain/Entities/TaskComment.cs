using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Taskit.Domain.Entities;

public class TaskComment : BaseEntity<int>
{
    [Required, MaxLength(1000)]
    public required string Content { get; set; }

    [Required, ForeignKey(nameof(Task))]
    public required int TaskId { get; set; }
    public required AppTask Task { get; set; }

    [Required, ForeignKey(nameof(Author))]
    public required string AuthorId { get; set; }
    public required AppUser Author { get; set; }
}
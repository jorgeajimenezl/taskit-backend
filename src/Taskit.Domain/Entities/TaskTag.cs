using System.ComponentModel.DataAnnotations;

namespace Taskit.Domain.Entities;

public class TaskTag : BaseEntity<int>
{
    [Required, MaxLength(50)]
    public required string Name { get; set; }

    [Required, StringLength(7)]
    public required string Color { get; set; } = "#000000";
}
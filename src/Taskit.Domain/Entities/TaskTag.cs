namespace Taskit.Domain.Entities;

public class TaskTag : BaseEntity
{
    public required string Name { get; set; }
    public required string Color { get; set; } = "#000000";
}
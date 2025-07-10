namespace Taskit.Models;

public abstract class BaseModel
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public void UpdateTimestamps()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
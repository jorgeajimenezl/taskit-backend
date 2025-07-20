namespace Taskit.Domain.Entities;

public class Media : BaseEntity<int>
{
    public Guid Uuid { get; set; }
    public required string CollectionName { get; set; }
    public required string Name { get; set; }
    public required string FileName { get; set; }
    public string? MimeType { get; set; }
    public required string Disk { get; set; }
    public ulong Size { get; set; }
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

    public int? ModelId { get; set; }
    public string? ModelType { get; set; }

    public string? UploadedById { get; set; }
    public AppUser? UploadedBy { get; set; }
}
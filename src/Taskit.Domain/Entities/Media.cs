using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Taskit.Domain.Entities;

public class Media : BaseEntity<int>
{
    [Required]
    public Guid Uuid { get; set; }

    [Required, MaxLength(100)]
    public required string CollectionName { get; set; }

    [Required, MaxLength(200)]
    public required string Name { get; set; }

    [Required, MaxLength(255)]
    public required string FileName { get; set; }

    [MaxLength(100)]
    public string? MimeType { get; set; }

    [Required, MaxLength(100)]
    public required string Disk { get; set; }

    public ulong Size { get; set; }

    [Column(TypeName = "jsonb")]
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

    public int? ModelId { get; set; }
    public string? ModelType { get; set; }

    [ForeignKey(nameof(UploadedBy))]
    public string? UploadedById { get; set; }
    public AppUser? UploadedBy { get; set; }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Taskit.Domain.Interfaces;
using Taskit.Domain.Enums;

namespace Taskit.Domain.Entities;

public class Media : BaseEntity<int>, ISoftDeletable
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

    public AccessScope AccessScope { get; set; } = AccessScope.Private;

    public IDictionary<string, object?>? Metadata { get; set; }

    public string? ModelId { get; set; }
    public string? ModelType { get; set; }

    [ForeignKey(nameof(UploadedBy))]
    public string? UploadedById { get; set; }
    public AppUser? UploadedBy { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
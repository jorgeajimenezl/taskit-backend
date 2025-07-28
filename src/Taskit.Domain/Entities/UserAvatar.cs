using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Taskit.Domain.Entities;

public class UserAvatar : BaseEntity<int>
{
    [Required, ForeignKey(nameof(Media))]
    public required int MediaId { get; set; }
    public Media? Media { get; set; }
}

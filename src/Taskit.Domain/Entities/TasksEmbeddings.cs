namespace Taskit.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pgvector;

public class TaskEmbeddings : BaseEntity<int>
{
    [Required, ForeignKey(nameof(Task))]
    public int TaskId { get; set; }
    public AppTask? Task { get; set; }

    [Column(TypeName = "vector(1536)")]
    public Vector? DescriptionEmbedding { get; set; }

    [Column(TypeName = "vector(500)")]
    public Vector? TitleEmbedding { get; set; }
}
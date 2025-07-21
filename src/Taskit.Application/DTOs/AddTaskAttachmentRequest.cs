using System.ComponentModel.DataAnnotations;

namespace Taskit.Application.DTOs;

public record AddTaskAttachmentRequest
{
    [Required]
    [Range(1, int.MaxValue)]
    public required int MediaId { get; init; }
}

using System.ComponentModel.DataAnnotations;

namespace Taskit.Application.DTOs;

public record AddTaskAttachmentRequest
{
    [Required]
    public required int MediaId { get; init; }
}

using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Taskit.Domain.Entities;


namespace Taskit.Application.DTOs;

public record CreateTaskRequest
{
    [Required]
    [StringLength(100)]
    public required string Title { get; init; }

    public string? Description { get; init; }

    public DateTime? DueDate { get; init; }

    public Taskit.Domain.Enums.TaskStatus Status { get; init; } = Taskit.Domain.Enums.TaskStatus.Created;

    public int Priority { get; init; } = 0;

    public int Complexity { get; init; } = 0;

    public int CompletedPercentage { get; init; } = 0;

    public int? ProjectId { get; init; }

    public string? AssignedUserId { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<CreateTaskRequest, AppTask>();
        }
    }
}

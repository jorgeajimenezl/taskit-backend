using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Taskit.Domain.Entities;
using TaskStatus = Taskit.Domain.Enums.TaskStatus;
using TaskPriority = Taskit.Domain.Enums.TaskPriority;

namespace Taskit.Application.DTOs;

public record CreateTaskRequest
{
    [Required]
    [StringLength(200)]
    public required string Title { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }

    public DateTime? DueDate { get; init; }

    public TaskStatus Status { get; init; } = TaskStatus.Created;

    public TaskPriority Priority { get; init; } = TaskPriority.Low;

    public int Complexity { get; init; } = 0;

    [Range(0, 100)]
    public int CompletedPercentage { get; init; } = 0;

    public int ProjectId { get; init; }

    public string? AssignedUserId { get; init; }

    public int? ParentTaskId { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<CreateTaskRequest, AppTask>();
        }
    }
}

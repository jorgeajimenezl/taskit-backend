using System.ComponentModel.DataAnnotations;


using AutoMapper;
using Taskit.Domain.Entities;
using TaskPriority = Taskit.Domain.Enums.TaskPriority;

namespace Taskit.Application.DTOs;

public record UpdateTaskRequest
{
    [StringLength(200)]
    public string? Title { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }

    public DateTime? DueDate { get; init; }

    public Taskit.Domain.Enums.TaskStatus? Status { get; init; }

    public TaskPriority? Priority { get; init; }

    public int? Complexity { get; init; }

    [Range(0, 100)]
    public int? CompletedPercentage { get; init; }

    public string? AssignedUserId { get; init; }

    public int? ParentTaskId { get; init; }

    public bool? IsArchived { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<UpdateTaskRequest, AppTask>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<TaskDto, UpdateTaskRequest>();
        }
    }
}

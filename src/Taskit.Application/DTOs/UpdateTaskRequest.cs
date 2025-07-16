using System.ComponentModel.DataAnnotations;


using AutoMapper;
using Taskit.Domain.Entities;

namespace Taskit.Application.DTOs;

public record UpdateTaskRequest
{
    [StringLength(100)]
    public string? Title { get; init; }

    public string? Description { get; init; }

    public DateTime? DueDate { get; init; }

    public Taskit.Domain.Enums.TaskStatus? Status { get; init; }

    public int? Priority { get; init; }

    public int? Complexity { get; init; }

    public int? CompletedPercentage { get; init; }

    public string? AssignedUserId { get; init; }

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

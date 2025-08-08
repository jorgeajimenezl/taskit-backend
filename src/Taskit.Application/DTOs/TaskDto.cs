using AutoMapper;
using System.ComponentModel.DataAnnotations;
using Taskit.Domain.Entities;

using TaskStatus = Taskit.Domain.Enums.TaskStatus;
using TaskPriority = Taskit.Domain.Enums.TaskPriority;
namespace Taskit.Application.DTOs;

public record TaskDto
{
    [Required]
    public int Id { get; init; }

    [Required]
    public required string Title { get; init; }

    [Required]
    public required string Description { get; init; }

    public string? GeneratedSummary { get; init; } = null;

    [Required]
    public required string ProjectName { get; init; }

    [Required]
    public int ProjectId { get; init; }

    public DateTime? DueDate { get; init; }
    public DateTime? CompletedAt { get; init; }
    public TaskStatus Status { get; init; }
    public TaskPriority Priority { get; init; }
    public int Complexity { get; init; }
    public int CompletedPercentage { get; init; }

    [Required]
    public DateTime CreatedAt { get; init; }

    [Required]
    public DateTime UpdatedAt { get; init; }

    public UserDto? Author { get; init; }
    public UserDto? AssignedUser { get; init; }
    public IEnumerable<TagDto> Tags { get; init; } = [];


    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<AppTask, TaskDto>()
                .ForMember(d => d.ProjectName, o => o.MapFrom(s => s.Project!.Name))
                .ForMember(d => d.ProjectId, o => o.MapFrom(s => s.ProjectId))
                .ForMember(d => d.AssignedUser, o => o.MapFrom(s => s.AssignedUser))
                .ForMember(d => d.Author, o => o.MapFrom(s => s.Author));
        }
    }
}
using AutoMapper;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Taskit.Domain.Entities;

using TaskStatus = Taskit.Domain.Enums.TaskStatus;
namespace Taskit.Application.DTOs;

public record TaskDto
{
    public int Id { get; init; }

    [Required]
    public required string Title { get; init; }

    [Required]
    public required string Description { get; init; }

    [Required]
    public required string ProjectName { get; init; }

    public int ProjectId { get; init; }
    public DateTime? DueDate { get; init; }
    public DateTime? CompletedAt { get; init; }
    public TaskStatus Status { get; init; }
    public int Priority { get; init; }
    public int Complexity { get; init; }
    public int CompletedPercentage { get; init; }

    public IEnumerable<TagDto> Tags { get; init; } = [];

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<AppTask, TaskDto>()
                .ForMember(d => d.ProjectName, o => o.MapFrom(s => s.Project!.Name))
                .ForMember(d => d.ProjectId, o => o.MapFrom(s => s.ProjectId));
        }
    }
}
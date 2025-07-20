using AutoMapper;
using System;
using Taskit.Domain.Entities;

namespace Taskit.Application.DTOs;

public record TaskDto
{
    public int Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public DateTime? DueDate { get; init; }
    public DateTime? CompletedAt { get; init; }
    public Taskit.Domain.Enums.TaskStatus Status { get; init; }

    public int Priority { get; init; }

    public int Complexity { get; init; }

    public int CompletedPercentage { get; init; }

    public IEnumerable<TagDto> Tags { get; init; } = Array.Empty<TagDto>();

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<AppTask, TaskDto>();
        }
    }
}
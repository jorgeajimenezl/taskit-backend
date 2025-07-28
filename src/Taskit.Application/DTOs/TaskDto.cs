using AutoMapper;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Text.Json.Serialization;
using Taskit.Domain.Entities;

using TaskStatus = Taskit.Domain.Enums.TaskStatus;
namespace Taskit.Application.DTOs;

public record TaskDto
{
    public int Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public DateTime? DueDate { get; init; }
    public DateTime? CompletedAt { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TaskStatus Status { get; init; }

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
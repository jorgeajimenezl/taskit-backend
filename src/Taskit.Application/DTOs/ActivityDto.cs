using AutoMapper;
using Taskit.Domain.Entities;
using Taskit.Domain.Enums;

namespace Taskit.Application.DTOs;

public record ActivityDto
{
    public int Id { get; init; }
    public DateTime Timestamp { get; init; }
    public ActivityEventType EventType { get; init; }
    public string? UserId { get; init; }
    public int? ProjectId { get; init; }
    public int? TaskId { get; init; }
    public IDictionary<string, object?> Data { get; init; } = new Dictionary<string, object?>();

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Activity, ActivityDto>();
        }
    }
}

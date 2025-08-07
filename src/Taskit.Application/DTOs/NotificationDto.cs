using AutoMapper;
using Taskit.Domain.Entities;
using Taskit.Domain.Enums;

namespace Taskit.Application.DTOs;

public record NotificationDto
{
    public int Id { get; init; }
    public string Title { get; init; } = default!;
    public string? Message { get; init; }
    public NotificationType Type { get; init; }
    public bool IsRead { get; init; }
    public IDictionary<string, object?>? Data { get; init; }
    public DateTime CreatedAt { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Notification, NotificationDto>();
        }
    }
}

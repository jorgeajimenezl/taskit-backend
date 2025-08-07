using AutoMapper;
using Gridify;
using Taskit.Application.Common.Mappings;
using Taskit.Application.DTOs;
using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;
using Taskit.Domain.Enums;

namespace Taskit.Application.Services;

public class NotificationService(
    INotificationRepository notificationRepository,
    IMapper mapper)
{
    private readonly INotificationRepository _notificationRepository = notificationRepository;
    private readonly IMapper _mapper = mapper;

    public async Task CreateAsync(string userId, string title, NotificationType type, string? message = null, IDictionary<string, object?>? data = null)
    {
        var notification = new Notification
        {
            Title = title,
            Message = message,
            Type = type,
            Data = data,
            UserId = userId
        };

        await _notificationRepository.AddAsync(notification);
    }

    public async Task<Paging<NotificationDto>> GetAllForUserAsync(string userId, IGridifyQuery query)
    {
        var q = _notificationRepository.QueryForUser(userId)
            .OrderByDescending(n => n.CreatedAt);

        return await q.GridifyToAsync<Notification, NotificationDto>(_mapper, query, GridifyMappings.NotificationMapper);
    }

    public async Task MarkAsReadAsync(int notificationId, string userId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);
        if (notification != null && notification.UserId == userId && !notification.IsRead)
        {
            notification.IsRead = true;
            await _notificationRepository.UpdateAsync(notification);
        }
    }

    public async Task DeleteAsync(int notificationId, string userId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);
        if (notification != null && notification.UserId == userId)
        {
            await _notificationRepository.DeleteAsync(notificationId);
        }
    }
}
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;
using Taskit.Domain.Events;
using Taskit.Infrastructure;

namespace Taskit.Notification.Worker.Consumers;

public class RealtimeNotificationConsumer(INotificationRepository notificationRepository)
    : IConsumer<ProjectActivityLogCreated>
{
    private readonly INotificationRepository _notificationRepository = notificationRepository;

    public Task Consume(ConsumeContext<ProjectActivityLogCreated> context)
    {
        // var msg = context.Message;
        // var exists = await _notificationRepository.GetByIdAsync(msg.NotificationId) != null;

        // if (!exists)
        // {
        //     var notification = new Taskit.Domain.Entities.Notification
        //     {
        //         Title = msg.Title,
        //         Message = msg.Message,
        //         Type = msg.Type,
        //         Data = msg.Data,
        //         UserId = msg.UserId
        //     };
        //     await _notificationRepository.AddAsync(notification, saveChanges: true);
        // }

        // Console.WriteLine($"Notification created: {msg.NotificationId}, User: {msg.UserId}, Title: {msg.Title}");
        return Task.CompletedTask;
    }
}
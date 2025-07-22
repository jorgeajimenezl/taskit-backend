using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;

namespace Taskit.Infrastructure.Repositories;

public class NotificationRepository(AppDbContext context) : Repository<Media, int>(context), INotificationRepository
{
}
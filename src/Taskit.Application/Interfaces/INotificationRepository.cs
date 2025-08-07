using System.Linq;
using Taskit.Domain.Entities;

namespace Taskit.Application.Interfaces;

public interface INotificationRepository : IRepository<Notification, int>
{
    IQueryable<Notification> QueryForUser(string userId);
}
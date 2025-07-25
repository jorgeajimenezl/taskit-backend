namespace Taskit.Notification.Worker.Interfaces;

using Taskit.Domain.Entities;
using Taskit.Domain.Events;

public interface IRecipientResolver<TEvent> where TEvent : IEvent<TEvent>
{
    Task<IEnumerable<AppUser>> GetRecipientsAsync(TEvent @event, CancellationToken cancellationToken = default);
}

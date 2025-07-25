namespace Taskit.Notification.Worker.Interfaces;

using Taskit.Domain.Events;

public interface IRecipientResolver<TEvent> where TEvent : IEvent<TEvent>
{
    Task<IEnumerable<string>> GetRecipientsAsync(TEvent @event, CancellationToken cancellationToken = default);
}

namespace Taskit.Notification.Worker.Services;

using Taskit.Domain.Events;

public interface IRecipientResolver
{
    Task<IEnumerable<string>> GetRecipientsAsync(ProjectActivityLogCreated @event, CancellationToken cancellationToken = default);
}

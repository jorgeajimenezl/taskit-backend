namespace Taskit.Domain.Events;

public interface IEvent<T>
{
    Guid Id { get; }
    DateTime Timestamp { get; }
}
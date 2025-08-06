namespace Taskit.Domain.Messages;

public interface IOperationResult
{
    // Guid CorrelationId { get; }
    DateTime Timestamp { get; }
}

public interface IOperationSucceeded<T> : IOperationResult
{
    T Result { get; }
}

public interface IOperationInProgress : IOperationResult
{

}

public record OperationSucceeded<T>(DateTime Timestamp, T Result)
    : IOperationSucceeded<T>;

public record OperationInProgress(DateTime Timestamp)
    : IOperationInProgress;
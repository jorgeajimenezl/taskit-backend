namespace Taskit.Domain.Messages;

public record OperationResult<TValue>
{
    public bool IsProcessing { get; init; }
    public TValue? Result { get; init; }

    public static OperationResult<TValue> Processing() => new() { IsProcessing = true };
    public static OperationResult<TValue> Success(TValue result) => new() { IsProcessing = false, Result = result };
}
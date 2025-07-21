namespace Taskit.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : this()
    {
        Errors = new Dictionary<string, string[]>(errors);
    }

    public IDictionary<string, string[]> Errors { get; init; }
}

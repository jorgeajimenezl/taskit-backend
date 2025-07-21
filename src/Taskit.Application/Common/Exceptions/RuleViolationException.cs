namespace Taskit.Application.Common.Exceptions;

public class RuleViolationException : InvalidOperationException
{
    public RuleViolationException(string message) : base(message) { }
}

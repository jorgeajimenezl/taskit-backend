namespace Taskit.Domain.Interfaces;

public interface IEmailSender
{
    Task SendAsync(string userId, string title, string? message);
}

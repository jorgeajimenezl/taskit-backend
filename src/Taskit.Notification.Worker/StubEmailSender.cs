using Taskit.Domain.Interfaces;

namespace Taskit.Notification.Worker;

public class StubEmailSender : IEmailSender
{
    public Task SendAsync(string userId, string title, string? message)
    {
        Console.WriteLine($"Email to {userId}: {title} - {message}");
        return Task.CompletedTask;
    }
}

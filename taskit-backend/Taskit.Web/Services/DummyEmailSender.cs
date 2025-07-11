using Microsoft.AspNetCore.Identity;
using Taskit.Domain.Entities;

namespace Taskit.Web.Services;

public class DummyEmailSender : IEmailSender<AppUser>
{
    public Task SendConfirmationLinkAsync(AppUser user, string email, string confirmationLink)
    {
        Console.WriteLine($"Confirmation link: {confirmationLink}");
        return Task.CompletedTask;
    }

    public Task SendPasswordResetLinkAsync(AppUser user, string email, string resetLink)
    {
        Console.WriteLine($"Password reset link: {resetLink}");
        return Task.CompletedTask;
    }

    public Task SendPasswordResetCodeAsync(AppUser user, string email, string resetCode)
    {
        Console.WriteLine($"Password reset code: {resetCode}");
        return Task.CompletedTask;
    }
}

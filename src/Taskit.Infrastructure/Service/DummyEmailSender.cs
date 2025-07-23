using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Taskit.Domain.Entities;

namespace Taskit.Infrastructure.Services;

public class DummyEmailSender(ILogger<DummyEmailSender> logger) : IEmailSender<AppUser>
{
    private readonly ILogger<DummyEmailSender> _logger = logger;

    public Task SendConfirmationLinkAsync(AppUser user, string email, string confirmationLink)
    {
        _logger.LogInformation("Confirmation link: {Link}", confirmationLink);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetLinkAsync(AppUser user, string email, string resetLink)
    {
        _logger.LogInformation("Password reset link: {Link}", resetLink);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetCodeAsync(AppUser user, string email, string resetCode)
    {
        _logger.LogInformation("Password reset code: {Code}", resetCode);
        return Task.CompletedTask;
    }
}

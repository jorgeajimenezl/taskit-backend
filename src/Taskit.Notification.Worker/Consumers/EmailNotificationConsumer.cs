using MassTransit;
using Taskit.Domain.Events;
using Taskit.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Taskit.Infrastructure;
using Taskit.Notification.Worker.Services;

namespace Taskit.Notification.Worker.Consumers;

public class EmailNotificationConsumer(
    AppDbContext db,
    IEmailSender emailSender,
    IEmailMessageGenerator messageGenerator,
    ILogger<EmailNotificationConsumer> logger) : IConsumer<ProjectActivityLogCreated>
{
    private readonly ILogger<EmailNotificationConsumer> _logger = logger;
    private readonly AppDbContext _db = db;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly IEmailMessageGenerator _messageGenerator = messageGenerator;

    public async Task Consume(ConsumeContext<ProjectActivityLogCreated> context)
    {
        var evt = context.Message;
        var recipients = await GetRecipientsAsync(evt, context.CancellationToken);

        foreach (var email in recipients)
        {
            var message = await _messageGenerator.GenerateAsync(evt, email, context.CancellationToken);
            await _emailSender.SendAsync(message, context.CancellationToken);
        }

        _logger.LogInformation("Processed email notification for event {Id}", evt.Id);
    }

    private async Task<IEnumerable<string>> GetRecipientsAsync(ProjectActivityLogCreated evt, CancellationToken ct)
    {
        if (evt.ProjectId is null)
            return [];

        var projectId = evt.ProjectId.Value;

        var memberEmails = await _db.ProjectMembers
            .Where(pm => pm.ProjectId == projectId)
            .Include(pm => pm.User)
            .Select(pm => pm.User!.Email)
            .ToListAsync(ct);

        var ownerEmail = await _db.Projects
            .Where(p => p.Id == projectId)
            .Select(p => p.Owner!.Email)
            .FirstOrDefaultAsync(ct);

        if (ownerEmail != null)
            memberEmails.Add(ownerEmail);

        var actorEmail = await _db.Users
            .Where(u => u.Id == evt.UserId)
            .Select(u => u.Email)
            .FirstOrDefaultAsync(ct);

        return memberEmails
            .Where(e => !string.IsNullOrWhiteSpace(e) && e != actorEmail)
            .Select(e => e!)
            .Distinct()
            .ToList();
    }
}
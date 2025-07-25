using Microsoft.EntityFrameworkCore;
using Taskit.Domain.Enums;
using Taskit.Domain.Events;
using Taskit.Infrastructure;
using Taskit.Notification.Worker.Interfaces;

namespace Taskit.Notification.Worker.Services;

public class DefaultRecipientResolver(AppDbContext db) : IRecipientResolver<ProjectActivityLogCreated>
{
    private readonly AppDbContext _db = db;

    public async Task<IEnumerable<string>> GetRecipientsAsync(ProjectActivityLogCreated evt, CancellationToken ct = default)
    {
        if (evt.ProjectId is null)
            return [];

        var actorEmail = await _db.Users
            .Where(u => u.Id == evt.UserId)
            .Select(u => u.Email)
            .FirstOrDefaultAsync(ct);

        if (evt.EventType == ProjectActivityLogEventType.TaskAssigned &&
            evt.Data?.TryGetValue("assignedTo", out var assignedUserIdObj) == true)
        {
            var assignedUserId = assignedUserIdObj?.ToString();
            if (assignedUserId is null)
                return [];

            var email = await _db.Users
                .Where(u => u.Id == assignedUserId)
                .Select(u => u.Email)
                .FirstOrDefaultAsync(ct);

            if (string.IsNullOrWhiteSpace(email) || email == actorEmail)
                return [];

            return new[] { email };
        }

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

        return memberEmails
            .Where(e => !string.IsNullOrWhiteSpace(e) && e != actorEmail)
            .Select(e => e!)
            .Distinct()
            .ToList();
    }
}

using Microsoft.EntityFrameworkCore;
using Taskit.Domain.Entities;
using Taskit.Domain.Enums;
using Taskit.Domain.Events;
using Taskit.Infrastructure;
using Taskit.Notification.Worker.Interfaces;

namespace Taskit.Notification.Worker.Services.RecipientResolver;

public class ProjectActivityLogRecipientResolver(AppDbContext db) : IRecipientResolver<ProjectActivityLogCreated>
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

        switch (evt.EventType)
        {
            case ProjectActivityLogEventType.TaskCreated:
            case ProjectActivityLogEventType.TaskUpdated:
            case ProjectActivityLogEventType.TaskDeleted:
            case ProjectActivityLogEventType.TaskStatusChanged:
                return [];

            case ProjectActivityLogEventType.CommentAdded:
            case ProjectActivityLogEventType.FileAttached:
            case ProjectActivityLogEventType.TaskAssigned:
                AppTask? task = await _db.Tasks
                        .Include(t => t.Project)
                        .FirstOrDefaultAsync(t => t.Id == evt.TaskId, ct);

                if (task is null || task.ProjectId != evt.ProjectId)
                    return [];

                var emails = await _db.Users
                    .Where(u => u.Id == task.AssignedUserId || u.Id == task.AuthorId)
                    .Select(u => u.Email!)
                    .ToListAsync(ct);

                var distinctEmails = emails
                    .Where(e => !string.IsNullOrWhiteSpace(e) && e != actorEmail)
                    .Distinct();

                return distinctEmails;
            case ProjectActivityLogEventType.ProjectCreated:
            case ProjectActivityLogEventType.ProjectUpdated:
            case ProjectActivityLogEventType.ProjectDeleted:
                return [];
            default:
                return [];
        }
    }
}

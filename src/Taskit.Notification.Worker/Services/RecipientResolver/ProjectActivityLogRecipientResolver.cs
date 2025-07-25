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

    public async Task<IEnumerable<AppUser>> GetRecipientsAsync(ProjectActivityLogCreated evt, CancellationToken ct = default)
    {
        if (evt.ProjectId is null)
            return [];

        var actor = await _db.Users
            .Where(u => u.Id == evt.UserId)
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

                var targetUsers = await _db.Users
                    .Where(u => u.Id == task.AssignedUserId || u.Id == task.AuthorId)
                    .ToListAsync(ct);

                if (actor is not null)
                {
                    targetUsers.RemoveAll(u => u.Id == actor.Id);
                }

                return targetUsers;
            case ProjectActivityLogEventType.ProjectCreated:
            case ProjectActivityLogEventType.ProjectUpdated:
            case ProjectActivityLogEventType.ProjectDeleted:
                return [];
            default:
                return [];
        }
    }
}

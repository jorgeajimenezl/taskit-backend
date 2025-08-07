using Microsoft.EntityFrameworkCore;
using Taskit.Domain.Entities;
using Taskit.Domain.Enums;
using Taskit.Domain.Events;
using Taskit.Infrastructure;
using Taskit.Notification.Worker.Interfaces;

namespace Taskit.Notification.Worker.Services.RecipientResolver;

public class TaskDuplicateRecipientResolver(AppDbContext db) : IRecipientResolver<TaskDuplicateDetected>
{
    private readonly AppDbContext _db = db;

    public async Task<IEnumerable<AppUser>> GetRecipientsAsync(TaskDuplicateDetected evt, CancellationToken ct = default)
    {
        var actor = await _db.Users
            .Where(u => u.Id == evt.UserId)
            .FirstOrDefaultAsync(ct);

        if (actor is null)
            return [];

        return [actor];
    }
}

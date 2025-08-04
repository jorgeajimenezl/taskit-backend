using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Gridify;
using Taskit.Application.Common.Mappings;
using Taskit.Application.Common.Models;
using Taskit.Application.Interfaces;
using Taskit.Application.DTOs;
using Taskit.Domain.Entities;
using Taskit.Domain.Enums;
using MassTransit;
using Taskit.Domain.Events;
using System.Threading;

namespace Taskit.Application.Services;

public class ProjectActivityLogService(
    IProjectActivityLogRepository activityRepository,
    IMapper mapper,
    IPublishEndpoint publisher)
{
    private readonly IPublishEndpoint _publisher = publisher;
    private readonly IProjectActivityLogRepository _activityLogs = activityRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<Paging<ProjectActivityLogDto>> GetForUserAsync(string userId, IGridifyQuery query, int? projectId = null, CancellationToken cancellationToken = default)
    {
        var q = _activityLogs.QueryForUser(userId).AsNoTracking();
        if (projectId != null)
            q = q.Where(a => a.ProjectId == projectId);
        q = q.OrderByDescending(a => a.Timestamp);
        return await q.GridifyToAsync<ProjectActivityLog, ProjectActivityLogDto>(_mapper, query, GridifyMappings.ProjectActivityLogMapper);
    }

    public async Task RecordAsync(
        ProjectActivityLogEventType eventType,
        string userId,
        int? projectId = null,
        int? taskId = null,
        IDictionary<string, object?>? data = null,
        CancellationToken cancellationToken = default)
    {
        var activity = new ProjectActivityLog
        {
            EventType = eventType,
            UserId = userId,
            ProjectId = projectId,
            TaskId = taskId,
            Data = data,
            Timestamp = DateTime.UtcNow
        };

        await _activityLogs.AddAsync(activity, saveChanges: false);
        await _publisher.Publish(new ProjectActivityLogCreated(
            Guid.NewGuid(),
            eventType,
            userId,
            projectId,
            taskId,
            activity.Data,
            activity.Timestamp), cancellationToken);
        await _activityLogs.SaveChangesAsync();
    }
}

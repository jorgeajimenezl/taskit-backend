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

namespace Taskit.Application.Services;

public class ProjectActivityLogService(IProjectActivityLogRepository activityRepository, IMapper mapper, NotificationService notificationService)
{
    private readonly NotificationService _notificationService = notificationService;
    private readonly IProjectActivityLogRepository _activityLogs = activityRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<Paging<ProjectActivityLogDto>> GetForUserAsync(string userId, IGridifyQuery query, int? projectId = null)
    {
        var q = _activityLogs.QueryForUser(userId);
        if (projectId != null)
            q = q.Where(a => a.ProjectId == projectId);
        q = q.OrderByDescending(a => a.Timestamp);
        return await q.GridifyToAsync<ProjectActivityLog, ProjectActivityLogDto>(_mapper, query);
    }

    public async Task RecordAsync(
        ProjectActivityLogEventType eventType,
        string userId,
        int? projectId = null,
        int? taskId = null,
        IDictionary<string, object?>? data = null)
    {
        var activity = new ProjectActivityLog
        {
            EventType = eventType,
            UserId = userId,
            ProjectId = projectId,
            TaskId = taskId,
            Data = data ?? new Dictionary<string, object?>(),
            Timestamp = DateTime.UtcNow
        };
        await _activityLogs.AddAsync(activity);

        switch (eventType)
        {
            case ProjectActivityLogEventType.TaskCreated:
                await _notificationService.CreateAsync(
                    userId,
                    "Task Created",
                    NotificationType.Info,
                    data: new Dictionary<string, object?>
                    {
                        { "taskId", taskId },
                        { "projectId", projectId }
                    });
                break;
            default:
                // Handle other event types as needed
                break;
        }
    }
}

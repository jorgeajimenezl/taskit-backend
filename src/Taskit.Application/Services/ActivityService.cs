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

public class ActivityService(IActivityRepository activityRepository, IMapper mapper)
{
    private readonly IActivityRepository _activities = activityRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<Paging<ActivityDto>> GetForUserAsync(string userId, IGridifyQuery query, int? projectId = null)
    {
        var q = _activities.QueryForUser(userId);
        if (projectId != null)
            q = q.Where(a => a.ProjectId == projectId);
        q = q.OrderByDescending(a => a.Timestamp);
        return await q.GridifyToAsync<Activity, ActivityDto>(_mapper, query);
    }

    public async Task RecordAsync(ActivityEventType eventType, string userId, int? projectId = null, int? taskId = null, IDictionary<string, object?>? data = null)
    {
        var activity = new Activity
        {
            EventType = eventType,
            UserId = userId,
            ProjectId = projectId,
            TaskId = taskId,
            Data = data ?? new Dictionary<string, object?>(),
            Timestamp = DateTime.UtcNow
        };
        await _activities.AddAsync(activity);
    }
}

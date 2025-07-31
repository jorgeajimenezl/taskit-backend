using AutoMapper;
using AutoMapper.QueryableExtensions;
using Gridify;
using Microsoft.EntityFrameworkCore;
using Ardalis.GuardClauses;
using Taskit.Application.Common.Mappings;
using Taskit.Application.Common.Models;
using Taskit.Application.DTOs;
using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;
using Taskit.Application.Common.Exceptions;
using Taskit.Domain.Enums;
using TaskStatus = Taskit.Domain.Enums.TaskStatus;

namespace Taskit.Application.Services;

public class TaskService(
    ITaskRepository taskRepository,
    IProjectRepository projectRepository,
    ITagRepository tagRepository,
    IMediaRepository mediaRepository,
    ProjectActivityLogService activityService,
    IMapper mapper)
{
    private readonly ITaskRepository _tasks = taskRepository;
    private readonly IProjectRepository _projects = projectRepository;
    private readonly ITagRepository _tagsRepo = tagRepository;
    private readonly IMediaRepository _mediaRepository = mediaRepository;
    private readonly ProjectActivityLogService _activity = activityService;
    private readonly IMapper _mapper = mapper;

    private async Task<AppTask?> GetAccessibleTaskWithTagsAsync(int taskId, string userId)
    {
        return await _tasks.QueryForUser(userId)
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == taskId);
    }

    public async Task<Paging<TaskDto>> GetAllForUserAsync(string userId, IGridifyQuery query)
    {
        return await _tasks.QueryForUser(userId)
            .Include(p => p.Project)
            .AsNoTracking()
            .GridifyToAsync<AppTask, TaskDto>(_mapper, query);
    }

    public async Task<TaskDto?> GetByIdAsync(int id, string userId)
    {
        return await _tasks.QueryForUser(userId)
            .Include(t => t.Project)
            .Where(t => t.Id == id)
            .AsNoTracking()
            .ProjectTo<TaskDto>(_mapper.ConfigurationProvider)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<TaskDto> CreateAsync(CreateTaskRequest dto, string userId)
    {
        var projectAllowed = await _projects.Query()
            .Include(p => p.Members)
            .AsNoTracking()
            .AnyAsync(p => p.Id == dto.ProjectId &&
                (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)));
        if (!projectAllowed)
            throw new ForbiddenAccessException();

        if (dto.ParentTaskId is not null)
        {
            var parentAllowed = await _tasks.QueryForUser(userId)
                .AsNoTracking()
                .AnyAsync(t => t.Id == dto.ParentTaskId);
            if (!parentAllowed)
                throw new ForbiddenAccessException();
        }

        if (dto.AssignedUserId is not null)
        {
            var assignedAllowed = await _projects.Query()
                .Include(p => p.Members)
                .AsNoTracking()
                .AnyAsync(p => p.Id == dto.ProjectId &&
                    p.Members.Any(m => m.UserId == dto.AssignedUserId));
            if (!assignedAllowed)
                throw new ForbiddenAccessException();
        }

        var task = _mapper.Map<AppTask>(dto);
        task.AuthorId = userId;

        await _tasks.AddAsync(task);
        await _activity.RecordAsync(ProjectActivityLogEventType.TaskCreated, userId, task.ProjectId, task.Id, new Dictionary<string, object?>
        {
            ["title"] = task.Title,
        });
        if (task.AssignedUserId != null)
        {
            await _activity.RecordAsync(ProjectActivityLogEventType.TaskAssigned, userId, task.ProjectId, task.Id, new Dictionary<string, object?>
            {
                ["assignedTo"] = task.AssignedUserId!
            });
        }

        return _mapper.Map<TaskDto>(task);
    }

    public async Task UpdateAsync(int id, UpdateTaskRequest dto, string userId)
    {
        var task = await _tasks.QueryForUser(userId)
            .Where(t => t.Id == id)
            .FirstOrDefaultAsync();

        Guard.Against.NotFound(id, task);
        var oldStatus = task.Status;

        _mapper.Map(dto, task);

        if (task.Status == TaskStatus.Completed && task.CompletedAt == null)
            task.CompletedAt = DateTime.UtcNow;
        else if (task.Status != TaskStatus.Completed)
            task.CompletedAt = null;

        task.UpdateTimestamps();
        await _tasks.UpdateAsync(task);

        if (task.Status != oldStatus)
        {
            await _activity.RecordAsync(ProjectActivityLogEventType.TaskStatusChanged, userId, task.ProjectId, task.Id, new Dictionary<string, object?>
            {
                ["status"] = task.Status.ToString(),
                ["assignedTo"] = task.AssignedUserId
            });
        }

        await _activity.RecordAsync(ProjectActivityLogEventType.TaskUpdated, userId, task.ProjectId, task.Id, new Dictionary<string, object?>
        {
            ["title"] = task.Title,
        });
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var task = await _tasks.GetByIdAsync(id);
        Guard.Against.NotFound(id, task);

        if (task.AuthorId != userId)
            throw new ForbiddenAccessException();

        await _tasks.DeleteAsync(id);
        await _activity.RecordAsync(ProjectActivityLogEventType.TaskDeleted, userId, task.ProjectId, id);
    }

    public async Task AddTagAsync(int taskId, int tagId, string userId)
    {
        var task = await GetAccessibleTaskWithTagsAsync(taskId, userId);
        Guard.Against.NotFound(taskId, task);

        if (task.Tags.Any(t => t.Id == tagId))
            return;

        var tag = await _tagsRepo.GetByIdAsync(tagId);
        Guard.Against.NotFound(tagId, tag);

        task.Tags.Add(tag);
        await _tasks.UpdateAsync(task);
    }

    public async Task RemoveTagAsync(int taskId, int tagId, string userId)
    {
        var task = await GetAccessibleTaskWithTagsAsync(taskId, userId);
        Guard.Against.NotFound(taskId, task);

        var tag = task.Tags.FirstOrDefault(t => t.Id == tagId);
        Guard.Against.NotFound(tagId, tag);

        task.Tags.Remove(tag);
        await _tasks.UpdateAsync(task);
    }

    public async Task<Paging<TaskDto>> GetByTagsAsync(IEnumerable<int> tagIds, string userId, IGridifyQuery query)
    {
        var tagIdSet = new HashSet<int>(tagIds);
        var queryable = _tasks.QueryForUser(userId).AsNoTracking();
        if (tagIds != null && tagIds.Any())
        {
            queryable = queryable.Where(t => t.Tags.Any(tag => tagIdSet.Contains(tag.Id)));
        }
        return await queryable.GridifyToAsync<AppTask, TaskDto>(_mapper, query);
    }

    public async Task AssignAsync(int taskId, string assignedUserId, string userId)
    {
        var task = await _tasks.QueryForUser(userId)
            .FirstOrDefaultAsync(t => t.Id == taskId);
        Guard.Against.NotFound(taskId, task);

        var assignedAllowed = await _projects.Query()
            .Include(p => p.Members)
            .AsNoTracking()
            .AnyAsync(p => p.Id == task.ProjectId && p.Members.Any(m => m.UserId == assignedUserId));
        if (!assignedAllowed)
            throw new ForbiddenAccessException();

        task.AssignedUserId = assignedUserId;
        task.UpdateTimestamps();

        await _tasks.UpdateAsync(task);
        await _activity.RecordAsync(ProjectActivityLogEventType.TaskAssigned, userId, task.ProjectId, task.Id, new Dictionary<string, object?>
        {
            ["assignedTo"] = assignedUserId,
        });
        await _activity.RecordAsync(ProjectActivityLogEventType.TaskUpdated, userId, task.ProjectId, task.Id, new Dictionary<string, object?>
        {
            ["title"] = task.Title,
        });
    }

    public async Task UnassignAsync(int taskId, string userId)
    {
        var task = await _tasks.QueryForUser(userId)
            .FirstOrDefaultAsync(t => t.Id == taskId);
        Guard.Against.NotFound(taskId, task);

        if (task.AssignedUserId == null)
            return;

        task.AssignedUserId = null;
        task.UpdateTimestamps();

        await _tasks.UpdateAsync(task);
        await _activity.RecordAsync(ProjectActivityLogEventType.TaskAssigned, userId, task.ProjectId, task.Id, new Dictionary<string, object?>
        {
            ["assignedTo"] = null,
        });
        await _activity.RecordAsync(ProjectActivityLogEventType.TaskUpdated, userId, task.ProjectId, task.Id, new Dictionary<string, object?>
        {
            ["title"] = task.Title,
        });
    }

    public async Task<IEnumerable<TaskDto>> GetSubTasksAsync(int taskId, string userId)
    {
        var subtasks = await _tasks.QueryForUser(userId)
            .Include(t => t.Project)
            .Where(t => t.ParentTaskId == taskId)
            .ProjectTo<TaskDto>(_mapper.ConfigurationProvider)
            .AsNoTracking()
            .ToListAsync();

        if (subtasks.Count == 0)
            throw new ForbiddenAccessException();

        return subtasks;
    }

    public async Task AttachSubTaskAsync(int parentTaskId, int subTaskId, string userId)
    {
        var subTask = await _tasks.QueryForUser(userId)
            .FirstOrDefaultAsync(t => t.Id == subTaskId);
        var parentTask = await _tasks.QueryForUser(userId)
            .FirstOrDefaultAsync(t => t.Id == parentTaskId);

        if (subTask?.ProjectId != parentTask?.ProjectId)
            throw new ForbiddenAccessException();

        Guard.Against.NotFound(parentTaskId, parentTask);
        Guard.Against.NotFound(subTaskId, subTask);

        subTask.ParentTaskId = parentTaskId;
        subTask.UpdateTimestamps();

        await _tasks.UpdateAsync(subTask);
        await _activity.RecordAsync(ProjectActivityLogEventType.TaskUpdated, userId, parentTask.ProjectId, parentTask.Id, new Dictionary<string, object?>
        {
            ["title"] = parentTask.Title,
            ["subTaskId"] = subTaskId,
        });
    }

    public async Task DetachSubTaskAsync(int parentTaskId, int subTaskId, string userId)
    {
        var subTask = await _tasks.QueryForUser(userId)
            .FirstOrDefaultAsync(t => t.Id == subTaskId && t.ParentTaskId == parentTaskId);
        Guard.Against.NotFound(subTaskId, subTask);

        subTask.ParentTaskId = null;
        subTask.UpdateTimestamps();

        await _tasks.UpdateAsync(subTask);
        await _activity.RecordAsync(ProjectActivityLogEventType.TaskUpdated, userId, subTask.ProjectId, subTask.Id, new Dictionary<string, object?>
        {
            ["title"] = subTask.Title,
            ["parentTaskId"] = null,
        });
    }

    private Task<bool> HasAccessToTaskAsync(int taskId, string userId)
    {
        return _tasks.QueryForUser(userId)
            .AsNoTracking()
            .AnyAsync(t => t.Id == taskId);
    }

    public async Task AttachMediaAsync(int taskId, int mediaId, string userId)
    {
        if (!await HasAccessToTaskAsync(taskId, userId))
            throw new ForbiddenAccessException();

        var media = await _mediaRepository.GetByIdAsync(mediaId);
        Guard.Against.NotFound(mediaId, media);

        if (media.UploadedById != userId)
            throw new ForbiddenAccessException();

        var task = await _tasks.QueryForUser(userId).FirstOrDefaultAsync(t => t.Id == taskId);
        Guard.Against.NotFound(taskId, task);

        media.ModelId = taskId.ToString();
        media.ModelType = nameof(AppTask);
        media.AccessScope = AccessScope.Private;
        await _mediaRepository.UpdateAsync(media);
        await _activity.RecordAsync(ProjectActivityLogEventType.FileAttached, userId, task.ProjectId, taskId, new Dictionary<string, object?>
        {
            ["mediaId"] = mediaId
        });
    }

    public async Task<IEnumerable<MediaDto>> GetAttachmentsAsync(int taskId, string userId)
    {
        if (!await HasAccessToTaskAsync(taskId, userId))
            throw new ForbiddenAccessException();

        var media = await _mediaRepository.Query()
            .Where(m => m.ModelType == nameof(AppTask) && m.ModelId == taskId.ToString())
            .AsNoTracking()
            .ToListAsync();

        return _mapper.Map<IEnumerable<MediaDto>>(media);
    }
}

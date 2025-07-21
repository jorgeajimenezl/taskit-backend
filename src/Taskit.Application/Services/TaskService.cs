using AutoMapper;
using AutoMapper.QueryableExtensions;
using Gridify;
using Gridify.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Ardalis.GuardClauses;
using Taskit.Application.Common.Mappings;
using Taskit.Application.Common.Models;
using Taskit.Application.DTOs;
using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;
using Taskit.Application.Common.Exceptions;

namespace Taskit.Application.Services;

public class TaskService(
    ITaskRepository taskRepository,
    IProjectRepository projectRepository,
    ITagRepository tagRepository,
    IMediaRepository mediaRepository,
    IMapper mapper)
{
    private readonly ITaskRepository _tasks = taskRepository;
    private readonly IProjectRepository _projects = projectRepository;
    private readonly ITagRepository _tagsRepo = tagRepository;
    private readonly IMediaRepository _mediaRepository = mediaRepository;
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
            .GridifyToAsync<AppTask, TaskDto>(_mapper, query);
    }

    public async Task<TaskDto?> GetByIdAsync(int id, string userId)
    {
        return await _tasks.QueryForUser(userId)
            .Where(t => t.Id == id)
            .ProjectTo<TaskDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
    }

    public async Task<TaskDto> CreateAsync(CreateTaskRequest dto, string userId)
    {
        if (dto.ProjectId is not null)
        {
            var projectAllowed = await _projects.Query()
                .Include(p => p.Members)
                .AnyAsync(p => p.Id == dto.ProjectId &&
                    (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)));
            if (!projectAllowed)
                throw new InvalidOperationException("Project not found or access denied");
        }

        if (dto.ParentTaskId is not null)
        {
            var parentAllowed = await _tasks.QueryForUser(userId)
                .AnyAsync(t => t.Id == dto.ParentTaskId);
            if (!parentAllowed)
                throw new InvalidOperationException("Parent task not found or access denied");
        }

        var task = _mapper.Map<AppTask>(dto);
        task.AuthorId = userId;

        await _tasks.AddAsync(task);
        return _mapper.Map<TaskDto>(task);
    }

    public async Task UpdateAsync(int id, UpdateTaskRequest dto, string userId)
    {
        var task = await _tasks.QueryForUser(userId)
            .Where(t => t.Id == id)
            .FirstOrDefaultAsync();
        if (task is null)
            throw new NotFoundException(nameof(AppTask), id.ToString());

        if (dto.ParentTaskId is not null)
        {
            var parentAllowed = await _tasks.QueryForUser(userId)
                .AnyAsync(t => t.Id == dto.ParentTaskId);
            if (!parentAllowed)
                throw new InvalidOperationException("Parent task not found or access denied");
        }

        _mapper.Map(dto, task);
        task.UpdateTimestamps();
        await _tasks.UpdateAsync(task);
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var task = await _tasks.GetByIdAsync(id);
        if (task is null)
            throw new NotFoundException(nameof(AppTask), id.ToString());
        if (task.AuthorId != userId)
            throw new ForbiddenAccessException();

        await _tasks.DeleteAsync(id);
    }

    public async Task AddTagAsync(int taskId, int tagId, string userId)
    {
        var task = await GetAccessibleTaskWithTagsAsync(taskId, userId);
        if (task is null)
            throw new NotFoundException(nameof(AppTask), taskId.ToString());

        if (task.Tags.Any(t => t.Id == tagId))
            return;

        var tag = await _tagsRepo.GetByIdAsync(tagId);
        if (tag is null)
            throw new NotFoundException(nameof(TaskTag), tagId.ToString());

        task.Tags.Add(tag);
        await _tasks.UpdateAsync(task);
    }

    public async Task RemoveTagAsync(int taskId, int tagId, string userId)
    {
        var task = await GetAccessibleTaskWithTagsAsync(taskId, userId);
        if (task is null)
            throw new NotFoundException(nameof(AppTask), taskId.ToString());

        var tag = task.Tags.FirstOrDefault(t => t.Id == tagId);
        if (tag is null)
            throw new NotFoundException(nameof(TaskTag), tagId.ToString());

        task.Tags.Remove(tag);
        await _tasks.UpdateAsync(task);
    }

    public async Task<Paging<TaskDto>> GetByTagsAsync(IEnumerable<int> tagIds, string userId, IGridifyQuery query)
    {
        var tagIdSet = new HashSet<int>(tagIds);
        var queryable = _tasks.QueryForUser(userId);
        if (tagIds != null && tagIds.Any())
        {
            queryable = queryable.Where(t => t.Tags.Any(tag => tagIdSet.Contains(tag.Id)));
        }
        return await queryable.GridifyToAsync<AppTask, TaskDto>(_mapper, query);
    }

    public async Task<IEnumerable<TaskDto>> GetSubTasksAsync(int taskId, string userId)
    {
        var subtasks = await _tasks.QueryForUser(userId)
            .Where(t => t.ParentTaskId == taskId)
            .ProjectTo<TaskDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        if (subtasks.Count == 0)
            throw new InvalidOperationException("Task not found or access denied");

        return subtasks;
    }

    public async Task DetachSubTaskAsync(int parentTaskId, int subTaskId, string userId)
    {
        var subTask = await _tasks.QueryForUser(userId)
            .FirstOrDefaultAsync(t => t.Id == subTaskId && t.ParentTaskId == parentTaskId);
        if (subTask is null)
            throw new NotFoundException(nameof(AppTask), subTaskId.ToString());

        subTask.ParentTaskId = null;
        subTask.UpdateTimestamps();
        await _tasks.UpdateAsync(subTask);
    }

    private Task<bool> HasAccessToTaskAsync(int taskId, string userId)
    {
        return _tasks.QueryForUser(userId).AnyAsync(t => t.Id == taskId);
    }

    public async Task<AttachMediaResult> AttachMediaAsync(int taskId, int mediaId, string userId)
    {
        if (!await HasAccessToTaskAsync(taskId, userId))
            return AttachMediaResult.TaskNotFound;

        var media = await _mediaRepository.GetByIdAsync(mediaId);
        if (media is null || media.UploadedById != userId)
            return AttachMediaResult.InvalidMedia;

        media.ModelId = taskId;
        media.ModelType = nameof(AppTask);
        await _mediaRepository.UpdateAsync(media);
        return AttachMediaResult.Success;
    }

    public async Task<IEnumerable<MediaDto>> GetAttachmentsAsync(int taskId, string userId)
    {
        if (!await HasAccessToTaskAsync(taskId, userId))
            throw new InvalidOperationException("Task not found or access denied");

        var media = await _mediaRepository.Query()
            .Where(m => m.ModelType == nameof(AppTask) && m.ModelId == taskId)
            .ToListAsync();

        return _mapper.Map<IEnumerable<MediaDto>>(media);
    }
}

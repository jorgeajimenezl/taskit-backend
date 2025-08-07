using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ardalis.GuardClauses;
using Gridify;
using Taskit.Application.Common.Exceptions;
using Taskit.Application.Common.Mappings;
using Taskit.Application.Common.Models;
using Taskit.Application.DTOs;
using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;
using Taskit.Domain.Enums;

namespace Taskit.Application.Services;

public class TaskCommentService(
    ITaskCommentRepository commentRepository,
    ITaskRepository taskRepository,
    IMapper mapper,
    ProjectActivityLogService activityService)
{
    private readonly ITaskCommentRepository _comments = commentRepository;
    private readonly ITaskRepository _tasks = taskRepository;
    private readonly IMapper _mapper = mapper;
    private readonly ProjectActivityLogService _activity = activityService;

    private async Task<bool> HasAccessToTask(int taskId, string userId)
    {
        return await _tasks.QueryForUser(userId)
            .AsNoTracking()
            .AnyAsync(t => t.Id == taskId);
    }

    public async Task<Paging<TaskCommentDto>> GetAllAsync(int taskId, string userId, IGridifyQuery query)
    {
        if (!await HasAccessToTask(taskId, userId))
            throw new ForbiddenAccessException();

        var q = _comments.QueryForTask(taskId)
            .Include(c => c.Author)
            .AsNoTracking();

        return await q.GridifyToAsync<TaskComment, TaskCommentDto>(_mapper, query, GridifyMappings.TaskCommentMapper);
    }

    public async Task<TaskCommentDto> GetByIdAsync(int taskId, int id, string userId)
    {
        if (!await HasAccessToTask(taskId, userId))
            throw new ForbiddenAccessException();

        var comment = await _comments.QueryForTask(taskId)
            .Include(c => c.Author)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        Guard.Against.NotFound(id, comment);
        return _mapper.Map<TaskCommentDto>(comment);
    }

    public async Task<TaskCommentDto> CreateAsync(int taskId, CreateTaskCommentRequest dto, string userId)
    {
        if (!await HasAccessToTask(taskId, userId))
            throw new ForbiddenAccessException();

        var comment = _mapper.Map<TaskComment>(dto);
        comment.TaskId = taskId;
        comment.AuthorId = userId;

        await _comments.AddAsync(comment);
        var projectId = await _tasks.Query()
            .AsNoTracking()
            .Where(t => t.Id == taskId)
            .Select(t => t.ProjectId)
            .FirstOrDefaultAsync();
        await _activity.RecordAsync(ProjectActivityLogEventType.CommentAdded, userId, projectId, taskId, new Dictionary<string, object?>
        {
            ["commentId"] = comment.Id
        });
        return _mapper.Map<TaskCommentDto>(comment);
    }

    public async Task UpdateAsync(int taskId, int id, UpdateTaskCommentRequest dto, string userId)
    {
        var comment = await _comments.QueryForTask(taskId)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
        Guard.Against.NotFound(id, comment);

        if (comment.AuthorId != userId || !await HasAccessToTask(taskId, userId))
            throw new ForbiddenAccessException();

        if ((DateTime.UtcNow - comment.CreatedAt).TotalMinutes > 30)
            throw new RuleViolationException("Comment can no longer be edited");

        _mapper.Map(dto, comment);
        comment.UpdateTimestamps();
        await _comments.UpdateAsync(comment);
    }

    public async Task DeleteAsync(int taskId, int id, string userId)
    {
        var comment = await _comments.QueryForTask(taskId)
            .FirstOrDefaultAsync(c => c.Id == id);
        Guard.Against.NotFound(id, comment);

        if (comment.AuthorId != userId || !await HasAccessToTask(taskId, userId))
            throw new ForbiddenAccessException();

        await _comments.DeleteAsync(id);
    }
}

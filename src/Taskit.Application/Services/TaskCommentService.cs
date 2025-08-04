using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Ardalis.GuardClauses;
using Taskit.Application.DTOs;
using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;
using Taskit.Application.Common.Exceptions;
using Taskit.Domain.Enums;
using System.Threading;

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

    private async Task<bool> HasAccessToTask(int taskId, string userId, CancellationToken cancellationToken = default)
    {
        return await _tasks.QueryForUser(userId)
            .AsNoTracking()
            .AnyAsync(t => t.Id == taskId, cancellationToken);
    }

    public async Task<IEnumerable<TaskCommentDto>> GetAllAsync(int taskId, string userId, CancellationToken cancellationToken = default)
    {
        if (!await HasAccessToTask(taskId, userId, cancellationToken))
            throw new ForbiddenAccessException();

        return await _comments.QueryForTask(taskId)
            .Include(c => c.Author)
            .AsNoTracking()
            .ProjectTo<TaskCommentDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task<TaskCommentDto> GetByIdAsync(int taskId, int id, string userId, CancellationToken cancellationToken = default)
    {
        if (!await HasAccessToTask(taskId, userId, cancellationToken))
            throw new ForbiddenAccessException();

        var comment = await _comments.QueryForTask(taskId)
            .Include(c => c.Author)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        Guard.Against.NotFound(id, comment);
        return _mapper.Map<TaskCommentDto>(comment);
    }

    public async Task<TaskCommentDto> CreateAsync(int taskId, CreateTaskCommentRequest dto, string userId, CancellationToken cancellationToken = default)
    {
        if (!await HasAccessToTask(taskId, userId, cancellationToken))
            throw new ForbiddenAccessException();

        var comment = _mapper.Map<TaskComment>(dto);
        comment.TaskId = taskId;
        comment.AuthorId = userId;

        await _comments.AddAsync(comment);
        var projectId = await _tasks.Query()
            .AsNoTracking()
            .Where(t => t.Id == taskId)
            .Select(t => t.ProjectId)
            .FirstOrDefaultAsync(cancellationToken);
        await _activity.RecordAsync(ProjectActivityLogEventType.CommentAdded, userId, projectId, taskId, new Dictionary<string, object?>
        {
            ["commentId"] = comment.Id
        }, cancellationToken);
        return _mapper.Map<TaskCommentDto>(comment);
    }

    public async Task UpdateAsync(int taskId, int id, UpdateTaskCommentRequest dto, string userId, CancellationToken cancellationToken = default)
    {
        var comment = await _comments.QueryForTask(taskId)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        Guard.Against.NotFound(id, comment);

        if (comment.AuthorId != userId || !await HasAccessToTask(taskId, userId, cancellationToken))
            throw new ForbiddenAccessException();

        if ((DateTime.UtcNow - comment.CreatedAt).TotalMinutes > 30)
            throw new RuleViolationException("Comment can no longer be edited");

        _mapper.Map(dto, comment);
        comment.UpdateTimestamps();
        await _comments.UpdateAsync(comment);
    }

    public async Task DeleteAsync(int taskId, int id, string userId, CancellationToken cancellationToken = default)
    {
        var comment = await _comments.QueryForTask(taskId)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        Guard.Against.NotFound(id, comment);

        if (comment.AuthorId != userId || !await HasAccessToTask(taskId, userId, cancellationToken))
            throw new ForbiddenAccessException();

        await _comments.DeleteAsync(id);
    }
}

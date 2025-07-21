using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Ardalis.GuardClauses;
using Taskit.Application.DTOs;
using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;
using Taskit.Application.Common.Exceptions;

namespace Taskit.Application.Services;

public class TaskCommentService(
    ITaskCommentRepository commentRepository,
    ITaskRepository taskRepository,
    IMapper mapper)
{
    private readonly ITaskCommentRepository _comments = commentRepository;
    private readonly ITaskRepository _tasks = taskRepository;
    private readonly IMapper _mapper = mapper;

    private async Task<bool> HasAccessToTask(int taskId, string userId)
    {
        return await _tasks.QueryForUser(userId)
            .AnyAsync(t => t.Id == taskId);
    }

    public async Task<IEnumerable<TaskCommentDto>> GetAllAsync(int taskId, string userId)
    {
        if (!await HasAccessToTask(taskId, userId))
            throw new InvalidOperationException("Task not found or access denied");

        return await _comments.QueryForTask(taskId)
            .Include(c => c.Author)
            .ProjectTo<TaskCommentDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<TaskCommentDto> GetByIdAsync(int taskId, int id, string userId)
    {
        if (!await HasAccessToTask(taskId, userId))
            throw new ForbiddenAccessException();

        var comment = await _comments.QueryForTask(taskId)
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comment is null)
            throw new NotFoundException(nameof(TaskComment), id.ToString());
        return _mapper.Map<TaskCommentDto>(comment);
    }

    public async Task<TaskCommentDto> CreateAsync(int taskId, CreateTaskCommentRequest dto, string userId)
    {
        if (!await HasAccessToTask(taskId, userId))
            throw new InvalidOperationException("Task not found or access denied");

        var comment = _mapper.Map<TaskComment>(dto);
        comment.TaskId = taskId;
        comment.AuthorId = userId;

        await _comments.AddAsync(comment);
        return _mapper.Map<TaskCommentDto>(comment);
    }

    public async Task UpdateAsync(int taskId, int id, UpdateTaskCommentRequest dto, string userId)
    {
        var comment = await _comments.QueryForTask(taskId)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (comment is null)
            throw new NotFoundException(nameof(TaskComment), id.ToString());
        if (comment.AuthorId != userId || !await HasAccessToTask(taskId, userId))
            throw new ForbiddenAccessException();

        _mapper.Map(dto, comment);
        comment.UpdateTimestamps();
        await _comments.UpdateAsync(comment);
    }

    public async Task DeleteAsync(int taskId, int id, string userId)
    {
        var comment = await _comments.QueryForTask(taskId)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (comment is null)
            throw new NotFoundException(nameof(TaskComment), id.ToString());
        if (comment.AuthorId != userId || !await HasAccessToTask(taskId, userId))
            throw new ForbiddenAccessException();

        await _comments.DeleteAsync(id);
    }
}

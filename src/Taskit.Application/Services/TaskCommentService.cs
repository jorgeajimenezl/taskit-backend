using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Taskit.Application.DTOs;
using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;

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

    public async Task<TaskCommentDto?> GetByIdAsync(int taskId, int id, string userId)
    {
        if (!await HasAccessToTask(taskId, userId))
            return null;

        var comment = await _comments.QueryForTask(taskId)
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.Id == id);
        return comment is null ? null : _mapper.Map<TaskCommentDto>(comment);
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

    public async Task<bool> UpdateAsync(int taskId, int id, UpdateTaskCommentRequest dto, string userId)
    {
        var comment = await _comments.QueryForTask(taskId)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (comment == null)
            return false;
        if (!await HasAccessToTask(taskId, userId) || comment.AuthorId != userId)
            return false;

        _mapper.Map(dto, comment);
        comment.UpdateTimestamps();
        await _comments.UpdateAsync(comment);
        return true;
    }

    public async Task<bool> DeleteAsync(int taskId, int id, string userId)
    {
        var comment = await _comments.QueryForTask(taskId)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (comment == null)
            return false;
        if (!await HasAccessToTask(taskId, userId) || comment.AuthorId != userId)
            return false;

        await _comments.DeleteAsync(id);
        return true;
    }
}

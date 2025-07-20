using AutoMapper;
using AutoMapper.QueryableExtensions;
using Gridify;
using Gridify.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Taskit.Application.Common.Mappings;
using Taskit.Application.Common.Models;
using Taskit.Application.DTOs;
using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;

namespace Taskit.Application.Services;

public class TaskService(
    ITaskRepository taskRepository,
    IProjectRepository projectRepository,
    ITagRepository tagRepository,
    IMapper mapper)
{
    private readonly ITaskRepository _tasks = taskRepository;
    private readonly IProjectRepository _projects = projectRepository;
    private readonly ITagRepository _tagsRepo = tagRepository;
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

        var task = _mapper.Map<AppTask>(dto);
        task.AuthorId = userId;

        await _tasks.AddAsync(task);
        return _mapper.Map<TaskDto>(task);
    }

    public async Task<bool> UpdateAsync(int id, UpdateTaskRequest dto, string userId)
    {
        var task = await _tasks.QueryForUser(userId)
            .Where(t => t.Id == id)
            .FirstOrDefaultAsync();
        if (task == null)
            return false;

        _mapper.Map(dto, task);
        task.UpdateTimestamps();
        await _tasks.UpdateAsync(task);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var task = await _tasks.GetByIdAsync(id);
        if (task == null || task.AuthorId != userId)
            return false;

        await _tasks.DeleteAsync(id);
        return true;
    }

    public async Task<bool> AddTagAsync(int taskId, int tagId, string userId)
    {
        var task = await GetAccessibleTaskWithTagsAsync(taskId, userId);
        if (task == null)
            return false;

        if (task.Tags.Any(t => t.Id == tagId))
            return true;

        var tag = await _tagsRepo.GetByIdAsync(tagId);
        if (tag == null)
            return false;

        task.Tags.Add(tag);
        await _tasks.UpdateAsync(task);
        return true;
    }

    public async Task<bool> RemoveTagAsync(int taskId, int tagId, string userId)
    {
        var task = await GetAccessibleTaskWithTagsAsync(taskId, userId);
        if (task == null)
            return false;

        var tag = task.Tags.FirstOrDefault(t => t.Id == tagId);
        if (tag == null)
            return false;

        task.Tags.Remove(tag);
        await _tasks.UpdateAsync(task);
        return true;
    }

    public async Task<Paging<TaskDto>> GetByTagsAsync(IEnumerable<int> tagIds, string userId, IGridifyQuery query)
    {
        var tagIdSet = new HashSet<int>(tagIds);
        return await _tasks.QueryForUser(userId)
            .Where(t => t.Tags.Any(tag => tagIdSet.Contains(tag.Id)))
            .GridifyToAsync<AppTask, TaskDto>(_mapper, query);
    }
}

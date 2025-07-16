using AutoMapper;
using Gridify;
using Gridify.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Taskit.Application.Common.Mappings;
using Taskit.Application.Common.Models;
using Taskit.Application.DTOs;
using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;

namespace Taskit.Application.Services;

public class TaskService(ITaskRepository taskRepository, IProjectRepository projectRepository, IMapper mapper)
{
    private readonly ITaskRepository _tasks = taskRepository;
    private readonly IProjectRepository _projects = projectRepository;
    private readonly IMapper _mapper = mapper;

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
                throw new InvalidOperationException("Invalid project");
        }

        var task = new AppTask
        {
            Title = dto.Title,
            Description = dto.Description ?? string.Empty,
            DueDate = dto.DueDate,
            Status = dto.Status,
            Priority = dto.Priority,
            Complexity = dto.Complexity,
            CompletedPercentage = dto.CompletedPercentage,
            AuthorId = userId,
            ProjectId = dto.ProjectId,
            AssignedUserId = dto.AssignedUserId
        };

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

        if (dto.Title is not null)
            task.Title = dto.Title;
        if (dto.Description is not null)
            task.Description = dto.Description;
        if (dto.DueDate.HasValue)
            task.DueDate = dto.DueDate;
        if (dto.Status.HasValue)
            task.Status = dto.Status.Value;
        if (dto.Priority.HasValue)
            task.Priority = dto.Priority.Value;
        if (dto.Complexity.HasValue)
            task.Complexity = dto.Complexity.Value;
        if (dto.CompletedPercentage.HasValue)
            task.CompletedPercentage = dto.CompletedPercentage.Value;
        if (dto.AssignedUserId is not null)
            task.AssignedUserId = dto.AssignedUserId;
        if (dto.IsArchived.HasValue)
            task.IsArchived = dto.IsArchived.Value;

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
}

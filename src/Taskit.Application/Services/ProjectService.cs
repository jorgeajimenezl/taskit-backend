using AutoMapper;
using AutoMapper.QueryableExtensions;
using Gridify;
using Microsoft.EntityFrameworkCore;
using Ardalis.GuardClauses;
using Taskit.Application.Common.Mappings;
using Taskit.Application.DTOs;
using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;
using Taskit.Application.Common.Exceptions;
using Taskit.Domain.Enums;

namespace Taskit.Application.Services;

public class ProjectService(IProjectRepository projectRepository, IMapper mapper, ActivityService activityService)
{
    private readonly ActivityService _activity = activityService;
    private readonly IProjectRepository _projects = projectRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<Paging<ProjectDto>> GetAllForUserAsync(string userId, IGridifyQuery query)
    {
        var q = _projects.Query().Include(p => p.Members)
            .Where(p => p.OwnerId == userId || p.Members.Any(m => m.UserId == userId));
        return await q.GridifyToAsync<Project, ProjectDto>(_mapper, query);
    }

    public async Task<ProjectDto> GetByIdAsync(int id, string userId)
    {
        var project = await _projects.Query().Include(p => p.Members)
            .Where(p => p.Id == id && (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)))
            .ProjectTo<ProjectDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
        Guard.Against.NotFound(id, project);
        return project;
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectRequest dto, string ownerId)
    {
        var project = _mapper.Map<Project>(dto);
        project.OwnerId = ownerId;

        await _projects.AddAsync(project);
        await _activity.RecordAsync(ActivityEventType.ProjectCreated, ownerId, project.Id, null, new Dictionary<string, object?>
        {
            ["projectId"] = project.Id,
            ["name"] = project.Name
        });

        return _mapper.Map<ProjectDto>(project);
    }

    public async Task UpdateAsync(int id, UpdateProjectRequest dto, string userId)
    {
        var project = await _projects.GetByIdAsync(id);
        Guard.Against.NotFound(id, project);

        if (project.OwnerId != userId)
            throw new ForbiddenAccessException();

        _mapper.Map(dto, project);
        project.UpdateTimestamps();

        await _projects.UpdateAsync(project);
        await _activity.RecordAsync(ActivityEventType.ProjectUpdated, userId, id, null, new Dictionary<string, object?>
        {
            ["projectId"] = id,
            ["name"] = project.Name
        });
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var project = await _projects.GetByIdAsync(id);
        Guard.Against.NotFound(id, project);

        if (project.OwnerId != userId)
            throw new ForbiddenAccessException();

        await _projects.DeleteAsync(id);
        await _activity.RecordAsync(ActivityEventType.ProjectDeleted, userId, id, null, new Dictionary<string, object?>
        {
            ["projectId"] = id,
            ["name"] = project.Name
        });
    }
}

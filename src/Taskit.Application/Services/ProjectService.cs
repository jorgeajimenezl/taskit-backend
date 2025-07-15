using AutoMapper;
using Gridify;
using Gridify.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Taskit.Application.Common.Mappings;
using Taskit.Application.DTOs;
using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;

namespace Taskit.Application.Services;

public class ProjectService(IProjectRepository projectRepository, IMapper mapper)
{
    private readonly IProjectRepository _projects = projectRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<Paging<ProjectDto>> GetAllForUserAsync(string userId, IGridifyQuery query)
    {
        var q = _projects.Query()
            .Where(p => p.OwnerId == userId || p.Members.Any(m => m.UserId == userId));
        return await q.GridifyToAsync<Project, ProjectDto>(_mapper, query);
    }

    public async Task<ProjectDto?> GetByIdAsync(int id, string userId)
    {
        var project = await _projects.Query()
            .Where(p => p.Id == id && (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)))
            .ProjectTo<ProjectDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
        return project;
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectRequest dto, string ownerId)
    {
        var project = new Project
        {
            Name = dto.Name,
            Description = dto.Description ?? string.Empty,
            OwnerId = ownerId
        };

        await _projects.AddAsync(project);
        return _mapper.Map<ProjectDto>(project);
    }

    public async Task<bool> UpdateAsync(int id, UpdateProjectRequest dto, string userId)
    {
        var project = await _projects.GetByIdAsync(id);
        if (project == null || project.OwnerId != userId)
            return false;

        if (dto.Name is not null)
            project.Name = dto.Name;
        if (dto.Description is not null)
            project.Description = dto.Description;

        project.UpdateTimestamps();
        await _projects.UpdateAsync(project);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var project = await _projects.GetByIdAsync(id);
        if (project == null || project.OwnerId != userId)
            return false;

        await _projects.DeleteAsync(id);
        return true;
    }
}

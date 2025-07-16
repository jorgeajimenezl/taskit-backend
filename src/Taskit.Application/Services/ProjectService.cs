using AutoMapper;
using AutoMapper.QueryableExtensions;
using Gridify;
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
        var q = _projects.Query().Include(p => p.Members)
            .Where(p => p.OwnerId == userId || p.Members.Any(m => m.UserId == userId));
        return await q.GridifyToAsync<Project, ProjectDto>(_mapper, query);
    }

    public async Task<ProjectDto?> GetByIdAsync(int id, string userId)
    {
        var project = await _projects.Query().Include(p => p.Members)
            .Where(p => p.Id == id && (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)))
            .ProjectTo<ProjectDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
        return project;
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectRequest dto, string ownerId)
    {
        var project = _mapper.Map<Project>(dto);
        project.OwnerId = ownerId;

        await _projects.AddAsync(project);
        return _mapper.Map<ProjectDto>(project);
    }

    public async Task<bool> UpdateAsync(int id, UpdateProjectRequest dto, string userId)
    {
        var project = await _projects.GetByIdAsync(id);
        if (project == null || project.OwnerId != userId)
            return false;

        _mapper.Map(dto, project);
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

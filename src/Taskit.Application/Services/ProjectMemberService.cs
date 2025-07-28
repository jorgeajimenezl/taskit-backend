using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ardalis.GuardClauses;
using Taskit.Application.DTOs;
using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;
using Taskit.Domain.Enums;
using Taskit.Application.Common.Exceptions;

namespace Taskit.Application.Services;

public class ProjectMemberService(
    IProjectMemberRepository memberRepository,
    IProjectRepository projectRepository,
    UserManager<AppUser> userManager,
    IMapper mapper,
    ProjectActivityLogService activityService)
{
    private readonly IProjectMemberRepository _members = memberRepository;
    private readonly IProjectRepository _projects = projectRepository;
    private readonly UserManager<AppUser> _users = userManager;
    private readonly IMapper _mapper = mapper;
    private readonly ProjectActivityLogService _activity = activityService;

    private static bool CanRead(Project project, string userId)
    {
        return project.OwnerId == userId || project.Members.Any(m => m.UserId == userId);
    }

    private static bool CanManage(Project project, string userId)
    {
        if (project.OwnerId == userId)
            return true;
        var member = project.Members.FirstOrDefault(m => m.UserId == userId);
        return member is not null && member.Role <= ProjectRole.Admin;
    }

    private async Task<Project?> GetProjectAsync(int projectId)
    {
        return await _projects.Query()
            .Include(p => p.Members)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == projectId);
    }

    public async Task<IEnumerable<ProjectMemberDto>> GetAllAsync(int projectId, string userId)
    {
        var project = await GetProjectAsync(projectId);
        Guard.Against.NotFound(projectId, project);

        if (!CanRead(project, userId))
            throw new ForbiddenAccessException();

        return await _members.QueryForProject(projectId)
            .Include(m => m.User)
            .AsNoTracking()
            .ProjectTo<ProjectMemberDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<ProjectMemberDto> GetByIdAsync(int projectId, int id, string userId)
    {
        var project = await GetProjectAsync(projectId);
        Guard.Against.NotFound(projectId, project);

        if (!CanRead(project, userId))
            throw new ForbiddenAccessException();

        var member = await _members.QueryForProject(projectId)
            .Include(m => m.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        Guard.Against.NotFound(id, member);
        return _mapper.Map<ProjectMemberDto>(member);
    }

    public async Task<ProjectMemberDto> AddAsync(int projectId, AddProjectMemberRequest dto, string userId)
    {
        var project = await GetProjectAsync(projectId);
        Guard.Against.NotFound(projectId, project);

        if (!CanManage(project, userId))
            throw new ForbiddenAccessException();

        if (project.Members.Any(m => m.UserId == dto.UserId))
            throw new RuleViolationException("User is already a member of this project");

        var user = await _users.FindByIdAsync(dto.UserId);
        Guard.Against.NotFound(dto.UserId, user);

        var member = _mapper.Map<ProjectMember>(dto);
        member.ProjectId = projectId;
        member.User = user;

        await _members.AddAsync(member);
        await _activity.RecordAsync(ProjectActivityLogEventType.UserJoinedProject, userId, projectId, null, new Dictionary<string, object?>
        {
            ["memberId"] = member.Id,
            ["addedUserId"] = dto.UserId
        });
        return _mapper.Map<ProjectMemberDto>(member);
    }

    public async Task UpdateAsync(int projectId, int id, UpdateProjectMemberRequest dto, string userId)
    {
        var member = await _members.Query()
            .Include(m => m.Project)
            .ThenInclude(p => p!.Members)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id && m.ProjectId == projectId);
        Guard.Against.NotFound(id, member);

        if (!CanManage(member.Project!, userId))
            throw new ForbiddenAccessException();

        if (dto.Role == ProjectRole.Owner)
            throw new RuleViolationException("Cannot change member role to Owner");

        _mapper.Map(dto, member);
        member.UpdateTimestamps();
        await _members.UpdateAsync(member);
    }

    public async Task DeleteAsync(int projectId, int id, string userId)
    {
        var member = await _members.Query()
            .Include(m => m.Project)
            .ThenInclude(p => p!.Members)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id && m.ProjectId == projectId);
        Guard.Against.NotFound(id, member);

        if (!CanManage(member.Project!, userId))
            throw new ForbiddenAccessException();

        await _members.DeleteAsync(id);
        await _activity.RecordAsync(ProjectActivityLogEventType.UserLeftProject, userId, projectId, null, new Dictionary<string, object?>
        {
            ["memberId"] = id
        });
    }
}

using System.Security.Claims;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskit.Application.DTOs;
using Taskit.Application.Common.Models;
using Taskit.Application.Services;

namespace Taskit.Web.Controllers;

[Authorize]
public class ProjectsController(ProjectService projectService) : ApiControllerBase
{
    private readonly ProjectService _projectService = projectService;

    [HttpGet(Name = "GetProjects")]
    public async Task<ActionResult<Paging<ProjectDto>>> GetProjects([FromQuery] GridifyQuery query)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var projects = await _projectService.GetAllForUserAsync(userId, query);
        return Ok(projects);
    }

    [HttpGet("{id:int}", Name = "GetProject")]
    public async Task<ActionResult<ProjectDto>> GetProject(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var project = await _projectService.GetByIdAsync(id, userId);
        return Ok(project);
    }

    [HttpPost(Name = "CreateProject")]
    public async Task<ActionResult<ProjectDto>> CreateProject(CreateProjectRequest dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var project = await _projectService.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
    }

    [HttpPut("{id:int}", Name = "UpdateProject")]
    public async Task<IActionResult> UpdateProject(int id, UpdateProjectRequest dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _projectService.UpdateAsync(id, dto, userId);
        return NoContent();
    }

    [HttpPatch("{id:int}", Name = "PatchProject")]
    public async Task<IActionResult> PatchProject(int id, UpdateProjectRequest dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _projectService.UpdateAsync(id, dto, userId);
        return NoContent();
    }

    [HttpDelete("{id:int}", Name = "DeleteProject")]
    public async Task<IActionResult> DeleteProject(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _projectService.DeleteAsync(id, userId);
        return NoContent();
    }
}

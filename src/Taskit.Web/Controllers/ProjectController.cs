using System.Security.Claims;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskit.Application.DTOs;
using Taskit.Application.Services;

namespace Taskit.Web.Controllers;

[Authorize]
public class ProjectController(ProjectService projectService) : ApiControllerBase
{
    private readonly ProjectService _projects = projectService;

    [HttpGet]
    public async Task<IActionResult> GetProjects([FromQuery] GridifyQuery query)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(await _projects.GetAllForUserAsync(userId, query));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectDto>> GetProject(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var project = await _projects.GetByIdAsync(id, userId);
        return project is null ? NotFound() : Ok(project);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> CreateProject(CreateProjectRequest dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var project = await _projects.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProject(int id, UpdateProjectRequest dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var project = await _projects.GetByIdAsync(id, userId);

        if (project is null)
        {
            return NotFound();
        }

        var success = await _projects.UpdateAsync(id, dto, userId);
        return success ? NoContent() : Forbid();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProject(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var project = await _projects.GetByIdAsync(id, userId);
        
        if (project is null)
        {
            return NotFound();
        }
        
        var success = await _projects.DeleteAsync(id, userId);
        return success ? NoContent() : Forbid();
    }
}

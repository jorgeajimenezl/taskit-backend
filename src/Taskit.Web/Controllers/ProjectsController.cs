using System.Security.Claims;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using AutoMapper;
using Taskit.Application.DTOs;
using Taskit.Application.Common.Models;
using Taskit.Application.Services;
using System.Threading;

namespace Taskit.Web.Controllers;

[Authorize]
public class ProjectsController(ProjectService projectService, IMapper mapper) : ApiControllerBase
{
    private readonly ProjectService _projectService = projectService;
    private readonly IMapper _mapper = mapper;

    [HttpGet(Name = "GetProjects")]
    public async Task<ActionResult<Paging<ProjectDto>>> GetProjects([FromQuery] GridifyQuery query, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var projects = await _projectService.GetAllForUserAsync(userId, query, cancellationToken);
        return Ok(projects);
    }

    [HttpGet("{id:int}", Name = "GetProject")]
    public async Task<ActionResult<ProjectDto>> GetProject(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var project = await _projectService.GetByIdAsync(id, userId, cancellationToken);
        return Ok(project);
    }

    [HttpPost(Name = "CreateProject")]
    public async Task<ActionResult<ProjectDto>> CreateProject(CreateProjectRequest dto, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var project = await _projectService.CreateAsync(dto, userId, cancellationToken);
        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
    }

    [HttpPut("{id:int}", Name = "UpdateProject")]
    public async Task<IActionResult> UpdateProject(int id, UpdateProjectRequest dto, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _projectService.UpdateAsync(id, dto, userId, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:int}", Name = "PatchProject")]
    public async Task<IActionResult> PatchProject(int id, JsonPatchDocument<UpdateProjectRequest> patch, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var project = await _projectService.GetByIdAsync(id, userId, cancellationToken);
        var dto = _mapper.Map<UpdateProjectRequest>(project);
        patch.ApplyTo(dto, (error) =>
        {
            var key = error.AffectedObject.GetType().Name;
            ModelState.AddModelError(key, error.ErrorMessage);
        });

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        await _projectService.UpdateAsync(id, dto, userId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}", Name = "DeleteProject")]
    public async Task<IActionResult> DeleteProject(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _projectService.DeleteAsync(id, userId, cancellationToken);
        return NoContent();
    }
}

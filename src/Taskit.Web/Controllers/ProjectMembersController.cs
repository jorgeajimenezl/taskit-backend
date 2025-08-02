using System.Security.Claims;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Taskit.Application.DTOs;
using Taskit.Application.Common.Models;
using Taskit.Application.Services;

namespace Taskit.Web.Controllers;

[Authorize]
[Route("api/projects/{projectId:int}/members")]
public class ProjectMembersController(ProjectMemberService service, IMapper mapper) : ApiControllerBase
{
    private readonly ProjectMemberService _service = service;
    private readonly IMapper _mapper = mapper;

    [HttpGet(Name = "GetProjectMembers")]
    public async Task<ActionResult<Paging<ProjectMemberDto>>> GetMembers(int projectId, [FromQuery] GridifyQuery query)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var members = await _service.GetAllAsync(projectId, userId, query);
        return Ok(members);
    }

    [HttpGet("{id:int}", Name = "GetProjectMember")]
    public async Task<ActionResult<ProjectMemberDto>> GetMember(int projectId, int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var member = await _service.GetByIdAsync(projectId, id, userId);
        return Ok(member);
    }

    [HttpPost(Name = "AddProjectMember")]
    public async Task<ActionResult<ProjectMemberDto>> AddMember(int projectId, AddProjectMemberRequest dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var member = await _service.AddAsync(projectId, dto, userId);
        return CreatedAtAction(nameof(GetMember), new { projectId, id = member.Id }, member);
    }

    [HttpPut("{id:int}", Name = "UpdateProjectMember")]
    public async Task<IActionResult> UpdateMember(int projectId, int id, UpdateProjectMemberRequest dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _service.UpdateAsync(projectId, id, dto, userId);
        return NoContent();
    }

    [HttpDelete("{id:int}", Name = "RemoveProjectMember")]
    public async Task<IActionResult> RemoveMember(int projectId, int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _service.DeleteAsync(projectId, id, userId);
        return NoContent();
    }
}

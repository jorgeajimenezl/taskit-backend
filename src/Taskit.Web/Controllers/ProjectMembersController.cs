using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Taskit.Application.DTOs;
using Taskit.Application.Services;

namespace Taskit.Web.Controllers;

[Authorize]
[Route("api/projects/{projectId:int}/members")]
public class ProjectMemberController(ProjectMemberService service, IMapper mapper) : ApiControllerBase
{
    private readonly ProjectMemberService _service = service;
    private readonly IMapper _mapper = mapper;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectMemberDto>>> GetMembers(int projectId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var members = await _service.GetAllAsync(projectId, userId);
        return Ok(members);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectMemberDto>> GetMember(int projectId, int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var member = await _service.GetByIdAsync(projectId, id, userId);
        return Ok(member);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectMemberDto>> AddMember(int projectId, AddProjectMemberRequest dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var member = await _service.AddAsync(projectId, dto, userId);
        return CreatedAtAction(nameof(GetMember), new { projectId, id = member.Id }, member);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateMember(int projectId, int id, UpdateProjectMemberRequest dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _service.UpdateAsync(projectId, id, dto, userId);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> RemoveMember(int projectId, int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _service.DeleteAsync(projectId, id, userId);
        return NoContent();
    }
}

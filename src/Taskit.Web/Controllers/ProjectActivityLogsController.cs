using System.Security.Claims;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskit.Application.DTOs;
using Taskit.Application.Services;

namespace Taskit.Web.Controllers;


[Authorize]
public class ProjectActivityLogsController(ProjectActivityLogService service) : ApiControllerBase
{
    private readonly ProjectActivityLogService _service = service;

    [HttpGet]
    public async Task<ActionResult<Paging<ProjectActivityLogDto>>> GetProjectActivityLogs([FromQuery] GridifyQuery query, [FromQuery] int? projectId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var activities = await _service.GetForUserAsync(userId, query, projectId);
        return Ok(activities);
    }
}

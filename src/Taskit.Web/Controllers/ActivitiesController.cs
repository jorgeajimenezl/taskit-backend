using System.Security.Claims;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskit.Application.DTOs;
using Taskit.Application.Services;

namespace Taskit.Web.Controllers;

[Authorize]
public class ActivitiesController(ActivityService service) : ApiControllerBase
{
    private readonly ActivityService _service = service;

    [HttpGet]
    public async Task<ActionResult<Paging<ActivityDto>>> GetActivities([FromQuery] GridifyQuery query, [FromQuery] int? projectId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var activities = await _service.GetForUserAsync(userId, query, projectId);
        return Ok(activities);
    }
}

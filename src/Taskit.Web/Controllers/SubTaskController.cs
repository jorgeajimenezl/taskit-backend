using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskit.Application.Services;

namespace Taskit.Web.Controllers;

[Authorize]
[Route("api/tasks/{taskId:int}/subtasks")]
public class SubTaskController(TaskService taskService) : ApiControllerBase
{
    private readonly TaskService _taskService = taskService;

    [HttpGet]
    public async Task<IActionResult> GetSubTasks(int taskId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var tasks = await _taskService.GetSubTasksAsync(taskId, userId);
        return Ok(tasks);
    }
}

using Gridify;
using Microsoft.AspNetCore.Mvc;
using Taskit.Application.Services;

namespace Taskit.Web.Controllers;

public class TaskController : ApiControllerBase
{
    private readonly TaskService _taskService;

    public TaskController(TaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet("")]
    public async Task<IActionResult> GetTasks([FromQuery] GridifyQuery query)
    {
        return Ok(await _taskService.GetAllAsync(query));
    }
}
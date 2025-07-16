using System.Security.Claims;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using Taskit.Application.DTOs;
using Taskit.Application.Services;

namespace Taskit.Web.Controllers;

[Authorize]
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
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(await _taskService.GetAllForUserAsync(userId, query));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskDto>> GetTask(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var task = await _taskService.GetByIdAsync(id, userId);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> CreateTask(CreateTaskRequest dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var task = await _taskService.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateTask(int id, UpdateTaskRequest dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var existing = await _taskService.GetByIdAsync(id, userId);
        if (existing is null)
            return NotFound();

        var success = await _taskService.UpdateAsync(id, dto, userId);
        return success ? NoContent() : Forbid();
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> PatchTask(int id, JsonPatchDocument<UpdateTaskRequest> patch)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var existing = await _taskService.GetByIdAsync(id, userId);
        if (existing is null)
            return NotFound();

        var dto = new UpdateTaskRequest
        {
            Title = existing.Title,
            Description = existing.Description,
            DueDate = existing.DueDate,
            Status = existing.Status,
            Priority = existing.Priority,
            Complexity = existing.Complexity,
            CompletedPercentage = existing.CompletedPercentage
        };

        patch.ApplyTo(dto, ModelState);

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var success = await _taskService.UpdateAsync(id, dto, userId);
        return success ? NoContent() : Forbid();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var existing = await _taskService.GetByIdAsync(id, userId);
        if (existing is null)
            return NotFound();

        var success = await _taskService.DeleteAsync(id, userId);
        return success ? NoContent() : Forbid();
    }
}

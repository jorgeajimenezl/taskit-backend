using System.Security.Claims;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using AutoMapper;
using Taskit.Application.DTOs;
using Taskit.Application.Services;

namespace Taskit.Web.Controllers;

[Authorize]
public class TaskController : ApiControllerBase
{
    private readonly TaskService _taskService;
    private readonly IMapper _mapper;

    public TaskController(TaskService taskService, IMapper mapper)
    {
        _taskService = taskService;
        _mapper = mapper;
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

        var dto = _mapper.Map<UpdateTaskRequest>(existing);

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

    [HttpPost("{id:int}/tags/{tagId:int}")]
    public async Task<IActionResult> AddTag(int id, int tagId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var success = await _taskService.AddTagAsync(id, tagId, userId);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}/tags/{tagId:int}")]
    public async Task<IActionResult> RemoveTag(int id, int tagId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var success = await _taskService.RemoveTagAsync(id, tagId, userId);
        return success ? NoContent() : NotFound();
    }

    [HttpGet("{id:int}/subtasks")]
    public async Task<IActionResult> GetSubTasks(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var tasks = await _taskService.GetSubTasksAsync(id, userId);
        return Ok(tasks);
    }

    [HttpDelete("{parentId:int}/subtasks/{subTaskId:int}")]
    public async Task<IActionResult> DetachSubTask(int parentId, int subTaskId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var success = await _taskService.DetachSubTaskAsync(parentId, subTaskId, userId);
        return success ? NoContent() : NotFound();
    }

    [HttpGet("by-tags")]
    public async Task<IActionResult> GetTasksByTags([FromQuery(Name = "ids")] int[] tagIds, [FromQuery] GridifyQuery query)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(await _taskService.GetByTagsAsync(tagIds, userId, query));
    }
}

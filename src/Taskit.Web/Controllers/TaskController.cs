using System.Security.Claims;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using AutoMapper;
using Taskit.Application.DTOs;
using Taskit.Application.Common.Models;
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
    public async Task<ActionResult<Paging<TaskDto>>> GetTasks([FromQuery] GridifyQuery query)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(await _taskService.GetAllForUserAsync(userId, query));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskDto>> GetTask(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var task = await _taskService.GetByIdAsync(id, userId);
        return Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask(CreateTaskRequest dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var task = await _taskService.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateTask(int id, UpdateTaskRequest dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _taskService.UpdateAsync(id, dto, userId);
        return NoContent();
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> PatchTask(int id, JsonPatchDocument<UpdateTaskRequest> patch)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var existing = await _taskService.GetByIdAsync(id, userId);
        var dto = _mapper.Map<UpdateTaskRequest>(existing);

        patch.ApplyTo(dto, ModelState);

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        await _taskService.UpdateAsync(id, dto, userId);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _taskService.DeleteAsync(id, userId);
        return NoContent();
    }

    [HttpPost("{id:int}/tags/{tagId:int}")]
    public async Task<IActionResult> AddTag(int id, int tagId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _taskService.AddTagAsync(id, tagId, userId);
        return NoContent();
    }

    [HttpDelete("{id:int}/tags/{tagId:int}")]
    public async Task<IActionResult> RemoveTag(int id, int tagId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _taskService.RemoveTagAsync(id, tagId, userId);
        return NoContent();
    }

    [HttpGet("{id:int}/subtasks")]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetSubTasks(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var tasks = await _taskService.GetSubTasksAsync(id, userId);
        return Ok(tasks);
    }

    [HttpDelete("{parentId:int}/subtasks/{subTaskId:int}")]
    public async Task<IActionResult> DetachSubTask(int parentId, int subTaskId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _taskService.DetachSubTaskAsync(parentId, subTaskId, userId);
        return NoContent();
    }

    [HttpGet("by-tags")]
    public async Task<ActionResult<Paging<TaskDto>>> GetTasksByTags([FromQuery(Name = "ids")] int[] tagIds, [FromQuery] GridifyQuery query)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(await _taskService.GetByTagsAsync(tagIds, userId, query));
    }

    [HttpPost("{taskId:int}/attachments")]
    public async Task<IActionResult> AddAttachment(int taskId, AddTaskAttachmentRequest dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _taskService.AttachMediaAsync(taskId, dto.MediaId, userId);
        return NoContent();
    }

    [HttpGet("{taskId:int}/attachments")]
    public async Task<ActionResult<IEnumerable<MediaDto>>> GetAttachments(int taskId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var attachments = await _taskService.GetAttachmentsAsync(taskId, userId);
        return Ok(attachments);
    }
}

using System.Security.Claims;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskit.Application.DTOs;
using Taskit.Application.Common.Models;
using Taskit.Application.Services;

namespace Taskit.Web.Controllers;

[Authorize]
public class TasksController(TaskService taskService) : ApiControllerBase
{
    private readonly TaskService _taskService = taskService;

    [HttpGet("", Name = "GetTasks")]
    public async Task<ActionResult<Paging<TaskDto>>> GetTasks([FromQuery] GridifyQuery query)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(await _taskService.GetAllForUserAsync(userId, query));
    }

    [HttpGet("{id:int}", Name = "GetTaskById")]
    public async Task<ActionResult<TaskDto>> GetTask(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var task = await _taskService.GetByIdAsync(id, userId);
        return Ok(task);
    }

    [HttpPost(Name = "CreateTask")]
    public async Task<ActionResult<TaskDto>> CreateTask(CreateTaskRequest dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var task = await _taskService.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }

    [HttpPut("{id:int}", Name = "UpdateTask")]
    public async Task<IActionResult> UpdateTask(int id, UpdateTaskRequest dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _taskService.UpdateAsync(id, dto, userId);
        return NoContent();
    }

    [HttpPatch("{id:int}", Name = "PatchTask")]
    public async Task<IActionResult> PatchTask(int id, UpdateTaskRequest dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _taskService.UpdateAsync(id, dto, userId);
        return NoContent();
    }

    [HttpDelete("{id:int}", Name = "DeleteTask")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _taskService.DeleteAsync(id, userId);
        return NoContent();
    }

    [HttpPost("{id:int}/tags/{tagId:int}", Name = "AddTagToTask")]
    public async Task<IActionResult> AddTag(int id, int tagId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _taskService.AddTagAsync(id, tagId, userId);
        return NoContent();
    }

    [HttpDelete("{id:int}/tags/{tagId:int}", Name = "RemoveTagFromTask")]
    public async Task<IActionResult> RemoveTag(int id, int tagId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _taskService.RemoveTagAsync(id, tagId, userId);
        return NoContent();
    }

    [HttpPost("{id:int}/assign/{assigneeId}", Name = "AssignTask")]
    public async Task<IActionResult> AssignTask(int id, string assigneeId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _taskService.AssignAsync(id, assigneeId, userId);
        return NoContent();
    }

    [HttpDelete("{id:int}/assign", Name = "UnassignTask")]
    public async Task<IActionResult> UnassignTask(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _taskService.UnassignAsync(id, userId);
        return NoContent();
    }

    [HttpGet("{id:int}/subtasks", Name = "GetSubTasks")]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetSubTasks(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var tasks = await _taskService.GetSubTasksAsync(id, userId);
        return Ok(tasks);
    }

    [HttpGet("{parentId:int}/subtasks/{subTaskId:int}", Name = "AttachSubTask")]
    public async Task<ActionResult<IEnumerable<TaskDto>>> AttachSubTask(int parentId, int subTaskId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _taskService.AttachSubTaskAsync(parentId, subTaskId, userId);
        return NoContent();
    }

    [HttpDelete("{parentId:int}/subtasks/{subTaskId:int}", Name = "DetachSubTask")]
    public async Task<IActionResult> DetachSubTask(int parentId, int subTaskId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _taskService.DetachSubTaskAsync(parentId, subTaskId, userId);
        return NoContent();
    }

    [HttpGet("by-tags", Name = "GetTasksByTags")]
    public async Task<ActionResult<Paging<TaskDto>>> GetTasksByTags([FromQuery(Name = "ids")] int[] tagIds, [FromQuery] GridifyQuery query)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(await _taskService.GetByTagsAsync(tagIds, userId, query));
    }

    [HttpPost("{taskId:int}/attachments", Name = "AddAttachmentToTask")]
    public async Task<IActionResult> AddAttachment(int taskId, AddTaskAttachmentRequest dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _taskService.AttachMediaAsync(taskId, dto.MediaId, userId);
        return NoContent();
    }

    [HttpGet("{taskId:int}/attachments", Name = "GetTaskAttachments")]
    public async Task<ActionResult<IEnumerable<MediaDto>>> GetAttachments(int taskId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var attachments = await _taskService.GetAttachmentsAsync(taskId, userId);
        return Ok(attachments);
    }
}

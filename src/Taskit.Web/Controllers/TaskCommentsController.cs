using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using AutoMapper;
using Taskit.Application.DTOs;
using Taskit.Application.Services;
using System.Threading;

namespace Taskit.Web.Controllers;

[Authorize]
[Route("api/tasks/{taskId:int}/comments")]
public class TaskCommentsController(TaskCommentService service, IMapper mapper) : ApiControllerBase
{
    private readonly TaskCommentService _service = service;
    private readonly IMapper _mapper = mapper;

    [HttpGet(Name = "GetTaskComments")]
    public async Task<ActionResult<IEnumerable<TaskCommentDto>>> GetComments(int taskId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var comments = await _service.GetAllAsync(taskId, userId, cancellationToken);
        return Ok(comments);
    }

    [HttpGet("{id:int}", Name = "GetTaskComment")]
    public async Task<ActionResult<TaskCommentDto>> GetComment(int taskId, int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var comment = await _service.GetByIdAsync(taskId, id, userId, cancellationToken);
        return Ok(comment);
    }

    [HttpPost(Name = "AddTaskComment")]
    public async Task<ActionResult<TaskCommentDto>> AddComment(int taskId, CreateTaskCommentRequest dto, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var comment = await _service.CreateAsync(taskId, dto, userId, cancellationToken);
        return CreatedAtAction(nameof(GetComment), new { taskId, id = comment.Id }, comment);
    }

    [HttpPut("{id:int}", Name = "UpdateTaskComment")]
    public async Task<IActionResult> UpdateComment(int taskId, int id, UpdateTaskCommentRequest dto, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _service.UpdateAsync(taskId, id, dto, userId, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:int}", Name = "PatchTaskComment")]
    public async Task<IActionResult> PatchComment(
        int taskId, int id,
        [FromBody] JsonPatchDocument<UpdateTaskCommentRequest> patch,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var existing = await _service.GetByIdAsync(taskId, id, userId, cancellationToken);
        var dto = _mapper.Map<UpdateTaskCommentRequest>(existing);
        patch.ApplyTo(dto, (error) =>
        {
            var key = error.AffectedObject.GetType().Name;
            ModelState.AddModelError(key, error.ErrorMessage);
        });

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        await _service.UpdateAsync(taskId, id, dto, userId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}", Name = "DeleteTaskComment")]
    public async Task<IActionResult> DeleteComment(int taskId, int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _service.DeleteAsync(taskId, id, userId, cancellationToken);
        return NoContent();
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using AutoMapper;
using Taskit.Application.DTOs;
using Taskit.Application.Services;

namespace Taskit.Web.Controllers;

[Authorize]
[Route("api/tasks/{taskId:int}/comments")]
public class TaskCommentController(TaskCommentService service, IMapper mapper) : ApiControllerBase
{
    private readonly TaskCommentService _service = service;
    private readonly IMapper _mapper = mapper;

    [HttpGet]
    public async Task<IActionResult> GetComments(int taskId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var comments = await _service.GetAllAsync(taskId, userId);
        return Ok(comments);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskCommentDto>> GetComment(int taskId, int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var comment = await _service.GetByIdAsync(taskId, id, userId);
        return comment is null ? NotFound() : Ok(comment);
    }

    [HttpPost]
    public async Task<ActionResult<TaskCommentDto>> AddComment(int taskId, CreateTaskCommentRequest dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var comment = await _service.CreateAsync(taskId, dto, userId);
        return CreatedAtAction(nameof(GetComment), new { taskId, id = comment.Id }, comment);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateComment(int taskId, int id, UpdateTaskCommentRequest dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var existing = await _service.GetByIdAsync(taskId, id, userId);
        if (existing is null)
            return NotFound();

        var success = await _service.UpdateAsync(taskId, id, dto, userId);
        return success ? NoContent() : Forbid();
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> PatchComment(int taskId, int id, JsonPatchDocument<UpdateTaskCommentRequest> patch)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var existing = await _service.GetByIdAsync(taskId, id, userId);
        if (existing is null)
            return NotFound();

        var dto = _mapper.Map<UpdateTaskCommentRequest>(existing);
        patch.ApplyTo(dto, ModelState);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var success = await _service.UpdateAsync(taskId, id, dto, userId);
        return success ? NoContent() : Forbid();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteComment(int taskId, int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var existing = await _service.GetByIdAsync(taskId, id, userId);
        if (existing is null)
            return NotFound();

        var success = await _service.DeleteAsync(taskId, id, userId);
        return success ? NoContent() : Forbid();
    }
}

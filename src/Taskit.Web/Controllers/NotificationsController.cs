using System.Security.Claims;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskit.Application.DTOs;
using Taskit.Application.Services;

namespace Taskit.Web.Controllers;

[Authorize]
public class NotificationsController(NotificationService service) : ApiControllerBase
{
    private readonly NotificationService _service = service;

    [HttpGet("", Name = "GetNotifications")]
    public async Task<ActionResult<Paging<NotificationDto>>> GetNotifications([FromQuery] GridifyQuery query)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var notifications = await _service.GetAllForUserAsync(userId, query);
        return Ok(notifications);
    }

    [HttpPut("{id:int}/read", Name = "MarkNotificationAsRead")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _service.MarkAsReadAsync(id, userId);
        return NoContent();
    }

    [HttpDelete("{id:int}", Name = "DeleteNotification")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _service.DeleteAsync(id, userId);
        return NoContent();
    }
}

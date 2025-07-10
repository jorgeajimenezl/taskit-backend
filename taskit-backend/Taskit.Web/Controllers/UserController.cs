using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Taskit.Models;
using Taskit.Services;

namespace Taskit.Controllers;

[Authorize]
public class UserController(UserManager<ApplicationUser> userManager) : ApiControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        return Ok(new
        {
            user.Id,
            user.FullName,
            user.UserName,
            user.Email,
        });
    }
}

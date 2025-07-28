using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Taskit.Application.DTOs;
using Taskit.Domain.Entities;

namespace Taskit.Web.Controllers;

[Authorize]
public class ProfileController(UserManager<AppUser> userManager) : ApiControllerBase
{
    private readonly UserManager<AppUser> _userManager = userManager;

    [HttpGet("me")]
    public async Task<ActionResult<ProfileResponse>> GetMe()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        return Ok(new ProfileResponse
        {
            FullName = user.FullName,
            Username = user.UserName!,
            Email = user.Email!,
            AvatarUrl = user.Avatar != null ? $"/media/{user.Avatar.FileName}" : null
        });
    }
}

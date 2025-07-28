using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Taskit.Application.DTOs;
using Taskit.Application.Services;
using Taskit.Domain.Entities;

namespace Taskit.Web.Controllers;

[Authorize]
public class ProfileController(UserManager<AppUser> userManager, UserAvatarService avatarService) : ApiControllerBase
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly UserAvatarService _avatars = avatarService;

    [HttpGet("me")]
    public async Task<ActionResult<ProfileResponse>> GetMe()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var user = await _userManager.Users
            .Include(u => u.Avatar)
                .ThenInclude(a => a.Media)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return NotFound();

        return Ok(new ProfileResponse
        {
            FullName = user.FullName,
            Username = user.UserName!,
            Email = user.Email!,
            AvatarUrl = user.Avatar?.Media != null ? $"/media/{user.Avatar.Media.FileName}" : null
        });
    }

    [HttpPost("avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        if (file == null)
            return BadRequest();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var url = await _avatars.UploadAsync(file, userId);
        return Ok(new { avatar_url = url });
    }
}

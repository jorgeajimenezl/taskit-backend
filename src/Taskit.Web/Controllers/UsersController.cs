using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Taskit.Application.DTOs;
using Taskit.Application.Services;
using Taskit.Domain.Entities;

namespace Taskit.Web.Controllers;

[Authorize]
public class UsersController(UserManager<AppUser> userManager, MediaService mediaService, IMapper mapper) : ApiControllerBase
{
    private readonly UserManager<AppUser> _users = userManager;
    private readonly MediaService _media = mediaService;
    private readonly IMapper _mapper = mapper;

    [HttpPost("{userId}/avatar")]
    public async Task<ActionResult<UserDto>> UploadAvatar(string userId, IFormFile file)
    {
        if (file == null)
            return BadRequest();

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (currentUserId != userId)
            return Forbid();

        var user = await _users.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        var media = await _media.UploadAsync(file, currentUserId, userId, nameof(AppUser), "avatars");
        user.AvatarId = media.Id;
        await _users.UpdateAsync(user);

        var dto = _mapper.Map<UserDto>(user);
        return Ok(dto);
    }
}

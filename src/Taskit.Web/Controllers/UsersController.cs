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

    [HttpPost("avatar")]
    public async Task<ActionResult<MediaDto>> UploadAvatar(IFormFile file)
    {
        if (file == null)
            return BadRequest();

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await _users.FindByIdAsync(currentUserId);
        if (user == null)
            return NotFound();

        var media = await _media.UploadAsync(file, currentUserId, currentUserId, nameof(AppUser), "avatars");
        user.AvatarId = media.Id;
        await _users.UpdateAsync(user);
        return Created($"/api/media/{media.Id}", media); ;
    }
}

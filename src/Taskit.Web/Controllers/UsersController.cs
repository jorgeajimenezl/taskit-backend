using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Taskit.Application.DTOs;
using Taskit.Application.Services;
using Taskit.Domain.Entities;
using Taskit.Domain.Enums;
using System.Threading;

namespace Taskit.Web.Controllers;

[Authorize]
public class UsersController(UserService userService) : ApiControllerBase
{
    private readonly UserService _userService = userService;

    [HttpPost("avatar", Name = "UploadAvatar")]
    public async Task<ActionResult<MediaDto>> UploadAvatar(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null)
            return BadRequest();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return await _userService.UploadAvatar(userId, file, cancellationToken);
    }

    [HttpGet("me", Name = "GetMe")]
    public async Task<ActionResult<UserProfileDto>> GetMe(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();
        ;
        var userDto = await _userService.GetUserByIdAsync(userId, cancellationToken);
        return Ok(userDto);
    }
}

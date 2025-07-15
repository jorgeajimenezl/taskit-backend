using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Taskit.Application.DTOs;
using Taskit.Domain.Entities;

namespace Taskit.Web.Controllers;

[Authorize]
public class ProfileController(UserManager<AppUser> userManager) : ApiControllerBase
{
    private readonly UserManager<AppUser> _userManager = userManager;

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest dto)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId == null)
            return Unauthorized();
        var user = await _userManager.FindByIdAsync(currentUserId);
        if (user == null)
            return NotFound();

        if (dto.FullName != null)
            user.FullName = dto.FullName;

        if (dto.Username != null)
            user.UserName = dto.Username;

        if (dto.Email != null)
            user.Email = dto.Email;
        {
            var emailResult = await _userManager.SetEmailAsync(user, dto.Email);
            if (!emailResult.Succeeded)
                return BadRequest(emailResult.Errors);
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProfile(string id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId == null)
            return Unauthorized();
        var user = await _userManager.FindByIdAsync(currentUserId);
        if (user == null)
            return NotFound();

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }
}
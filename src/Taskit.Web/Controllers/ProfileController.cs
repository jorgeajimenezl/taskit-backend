using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Taskit.Application.DTOs;
using Taskit.Domain.Entities;

namespace Taskit.Web.Controllers;

[Authorize]
public class ProfileController(UserManager<AppUser> userManager) : ApiControllerBase
{
    private readonly UserManager<AppUser> _userManager = userManager;

    [HttpGet]
    public async Task<IActionResult> GetProfiles()
    {
        var users = await _userManager.Users
            .Select(u => new { u.Id, u.FullName, u.UserName, u.Email })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProfile(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        return Ok(new { user.Id, user.FullName, user.UserName, user.Email });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProfile(string id, UpdateProfileRequest dto)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        if (dto.FullName != null)
            user.FullName = dto.FullName;
        if (dto.Username != null)
            user.UserName = dto.Username;
        if (dto.Email != null)
            user.Email = dto.Email;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProfile(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }
}

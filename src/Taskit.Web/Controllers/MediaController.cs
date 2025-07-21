using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Taskit.Application.DTOs;
using Taskit.Application.Services;

namespace Taskit.Web.Controllers;

[Authorize]
public class MediaController(MediaService mediaService) : ApiControllerBase
{
    private readonly MediaService _mediaService = mediaService;

    [HttpPost]
    public async Task<ActionResult<MediaDto>> Upload(IFormFile file)
    {
        if (file == null)
            return BadRequest();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var media = await _mediaService.UploadAsync(file, userId);
        return Created($"/api/media/{media.Id}", media);
    }

    [HttpDelete("{mediaId:int}")]
    public async Task<IActionResult> Delete(int mediaId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _mediaService.DeleteAsync(mediaId, userId);
        return NoContent();
    }
}


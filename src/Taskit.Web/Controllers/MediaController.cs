using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Taskit.Application.DTOs;
using Taskit.Application.Services;
using Taskit.Domain.Enums;

namespace Taskit.Web.Controllers;

[Authorize]
public class MediaController(MediaService mediaService) : ApiControllerBase
{
    private readonly MediaService _mediaService = mediaService;

    [AllowAnonymous]
    [HttpGet("{mediaId:int}")]
    public async Task<IActionResult> Download(int mediaId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var (path, mime) = await _mediaService.GetFileAsync(mediaId, userId);
        return PhysicalFile(path, mime, enableRangeProcessing: true);
    }

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


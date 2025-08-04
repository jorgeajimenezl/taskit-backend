using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Taskit.Application.DTOs;
using Taskit.Application.Services;
using Taskit.Domain.Enums;
using System.Threading;

namespace Taskit.Web.Controllers;

[Authorize]
public class MediaController(MediaService mediaService) : ApiControllerBase
{
    private readonly MediaService _mediaService = mediaService;

    [AllowAnonymous]
    [HttpGet("{mediaId:int}", Name = "GetMediaFile")]
    public async Task<IActionResult> Download(int mediaId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var (path, mime) = await _mediaService.GetFileAsync(mediaId, userId, cancellationToken);
        return PhysicalFile(path, mime, enableRangeProcessing: true);
    }

    [HttpPost("upload", Name = "UploadMediaFile")]
    public async Task<ActionResult<MediaDto>> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null)
            return BadRequest();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var media = await _mediaService.UploadAsync(file, userId, cancellationToken: cancellationToken);
        return Created($"http://localhost:5152/api/media/{media.Id}", media);
    }

    [HttpDelete("{mediaId:int}", Name = "DeleteMediaFile")]
    public async Task<IActionResult> Delete(int mediaId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _mediaService.DeleteAsync(mediaId, userId, cancellationToken);
        return NoContent();
    }
}


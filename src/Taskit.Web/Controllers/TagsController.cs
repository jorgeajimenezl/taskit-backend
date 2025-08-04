using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskit.Application.DTOs;
using Taskit.Application.Services;
using System.Threading;

namespace Taskit.Web.Controllers;

[Authorize]
public class TagsController(TagService service) : ApiControllerBase
{
    private readonly TagService _service = service;

    [HttpGet(Name = "GetTags")]
    public async Task<ActionResult<IEnumerable<TagDto>>> GetTags(CancellationToken cancellationToken)
    {
        var tags = await _service.GetAllAsync(cancellationToken);
        return Ok(tags);
    }

    [HttpPost(Name = "CreateTag")]
    public async Task<ActionResult<TagDto>> CreateTag(CreateTagRequest dto, CancellationToken cancellationToken)
    {
        var tag = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetTags), new { id = tag.Id }, tag);
    }
}

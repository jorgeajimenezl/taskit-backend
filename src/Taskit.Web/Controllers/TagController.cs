using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskit.Application.DTOs;
using Taskit.Application.Services;

namespace Taskit.Web.Controllers;

[Authorize]
public class TagController(TagService service) : ApiControllerBase
{
    private readonly TagService _service = service;

    [HttpGet]
    public async Task<IEnumerable<TagDto>> GetTags()
    {
        return await _service.GetAllAsync();
    }

    [HttpPost]
    public async Task<ActionResult<TagDto>> CreateTag(CreateTagRequest dto)
    {
        var tag = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetTags), new { id = tag.Id }, tag);
    }
}

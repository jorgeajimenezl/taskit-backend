using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Taskit.Application.DTOs;
using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;

namespace Taskit.Application.Services;

public class TagService(ITagRepository tagRepository, IMapper mapper)
{
    private readonly ITagRepository _tags = tagRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<TagDto>> GetAllAsync()
    {
        var tags = await _tags.Query()
            .AsNoTracking()
            .ToListAsync();
        return _mapper.Map<IEnumerable<TagDto>>(tags);
    }

    public async Task<TagDto> CreateAsync(CreateTagRequest dto)
    {
        var tag = _mapper.Map<TaskTag>(dto);
        await _tags.AddAsync(tag);
        return _mapper.Map<TagDto>(tag);
    }
}

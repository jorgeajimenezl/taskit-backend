using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Taskit.Application.DTOs;
using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;
using System.Threading;

namespace Taskit.Application.Services;

public class TagService(ITagRepository tagRepository, IMapper mapper)
{
    private readonly ITagRepository _tags = tagRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<TagDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tags = await _tags.Query()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<TagDto>>(tags);
    }

    public async Task<TagDto> CreateAsync(CreateTagRequest dto, CancellationToken cancellationToken = default)
    {
        var tag = _mapper.Map<TaskTag>(dto);
        await _tags.AddAsync(tag);
        return _mapper.Map<TagDto>(tag);
    }
}

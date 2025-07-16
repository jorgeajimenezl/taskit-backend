using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;

namespace Taskit.Application.Services;

public class MediaService(IMediaRepository mediaRepository)
{
    private readonly IMediaRepository _mediaRepository = mediaRepository;
}
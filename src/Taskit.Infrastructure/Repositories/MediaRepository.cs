using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;

namespace Taskit.Infrastructure.Repositories;

public class MediaRepository(AppDbContext context) : Repository<Media, int>(context), IMediaRepository
{
}
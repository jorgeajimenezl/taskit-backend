using Taskit.Domain.Entities;
using Taskit.Application.Interfaces;

namespace Taskit.Infrastructure.Repositories;

public class TagRepository(AppDbContext context) : Repository<TaskTag, int>(context), ITagRepository
{
}

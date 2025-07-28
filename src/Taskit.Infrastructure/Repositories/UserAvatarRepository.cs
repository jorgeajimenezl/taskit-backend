using Taskit.Domain.Entities;
using Taskit.Application.Interfaces;

namespace Taskit.Infrastructure.Repositories;

public class UserAvatarRepository(AppDbContext context) : Repository<UserAvatar, int>(context), IUserAvatarRepository
{
}

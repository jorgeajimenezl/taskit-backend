using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;

namespace Taskit.Application.Services;

public class UserAvatarService(
    IUserAvatarRepository avatarRepository,
    MediaService mediaService,
    UserManager<AppUser> userManager)
{
    private readonly IUserAvatarRepository _avatars = avatarRepository;
    private readonly MediaService _media = mediaService;
    private readonly UserManager<AppUser> _users = userManager;

    public async Task<string> UploadAsync(IFormFile file, string userId)
    {
        var user = await _users.Users
            .Include(u => u.Avatar)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            throw new UnauthorizedAccessException();

        var media = await _media.UploadAsync(file, userId, null, null, "avatars");
        var avatar = new UserAvatar { MediaId = media.Id };
        await _avatars.AddAsync(avatar);

        user.AvatarId = avatar.Id;
        await _users.UpdateAsync(user);

        return media.Url;
    }
}

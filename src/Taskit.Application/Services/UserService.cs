using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Taskit.Application.DTOs;
using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;
using Taskit.Domain.Enums;
using System.Threading;

namespace Taskit.Application.Services;

public class UserService(UserManager<AppUser> userManager, IMapper mapper, MediaService service)
{
    private readonly UserManager<AppUser> _users = userManager;
    private readonly IMapper _mapper = mapper;
    private readonly MediaService _media = service;

    public async Task<MediaDto> UploadAvatar(string userId, IFormFile file, CancellationToken cancellationToken = default)
    {
        var user = await _users.FindByIdAsync(userId)
            ?? throw new UnauthorizedAccessException("User not found");

        var mediaDto = await _media.UploadAsync(
            file,
            userId,
            userId,
            nameof(AppUser),
            "avatars",
            AccessScope.Public,
            cancellationToken: cancellationToken
        );

        user.AvatarId = mediaDto.Id;
        await _users.UpdateAsync(user);
        return mediaDto;
    }

    public async Task<UserProfileDto> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _users.Users
            .Include(u => u.Avatar)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found");

        return _mapper.Map<UserProfileDto>(user);
    }
}

using System.ComponentModel.DataAnnotations;

namespace Taskit.Application.DTOs;

public record UserProfileDto
{
    [Required]
    public required string Id { get; init; }

    [Required]
    public required string UserName { get; init; }

    [Required]
    public required string Email { get; init; }

    [Required]
    public required string FullName { get; init; }

    public string? AvatarUrl { get; init; }

    private class Mapping : AutoMapper.Profile
    {
        public Mapping()
        {
            CreateMap<Taskit.Domain.Entities.AppUser, UserProfileDto>()
                .ForMember(d => d.AvatarUrl, opt => opt.MapFrom(s => s.Avatar != null ? $"http://localhost:5152/media/{s.Avatar.Id}" : null));
        }
    }
}
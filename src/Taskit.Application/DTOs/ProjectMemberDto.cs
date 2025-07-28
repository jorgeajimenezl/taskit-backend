using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Taskit.Domain.Entities;
using Taskit.Domain.Enums;

namespace Taskit.Application.DTOs;

public record ProjectMemberDto
{
    public int Id { get; init; }

    [Required]
    public required string UserId { get; init; }

    [Required]
    public required string Username { get; init; }

    public string? FullName { get; init; }
    public string? AvatarUrl { get; init; }
    public ProjectRole Role { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ProjectMember, ProjectMemberDto>()
                .ForMember(d => d.Username, o => o.MapFrom(s => s.User!.UserName))
                .ForMember(d => d.FullName, o => o.MapFrom(s => s.User!.FullName))
                .ForMember(d => d.AvatarUrl,
                    o => o.MapFrom(s => s.User!.Avatar != null && s.User.Avatar.Media != null
                        ? $"/media/{s.User.Avatar.Media.FileName}"
                        : null));
        }
    }
}

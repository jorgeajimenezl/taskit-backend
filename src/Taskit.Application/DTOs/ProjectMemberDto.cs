using AutoMapper;
using Taskit.Domain.Entities;
using Taskit.Domain.Enums;

namespace Taskit.Application.DTOs;

public record ProjectMemberDto
{
    public int Id { get; init; }
    public required string UserId { get; init; }
    public required string Username { get; init; }
    public string? FullName { get; init; }
    public ProjectRole Role { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ProjectMember, ProjectMemberDto>()
                .ForMember(d => d.Username, o => o.MapFrom(s => s.User.UserName))
                .ForMember(d => d.FullName, o => o.MapFrom(s => s.User.FullName));
        }
    }
}

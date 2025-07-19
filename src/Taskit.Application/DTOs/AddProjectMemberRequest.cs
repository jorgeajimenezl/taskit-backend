using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Taskit.Domain.Entities;
using Taskit.Domain.Enums;

namespace Taskit.Application.DTOs;

public record AddProjectMemberRequest
{
    [Required]
    public required string UserId { get; init; }

    public ProjectRole Role { get; init; } = ProjectRole.Member;

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<AddProjectMemberRequest, ProjectMember>();
        }
    }
}

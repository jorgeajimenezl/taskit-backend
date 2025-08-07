using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Taskit.Domain.Entities;
using Taskit.Domain.Enums;

namespace Taskit.Application.DTOs;

public record ProjectMemberDto
{
    [Required]
    public int Id { get; init; }

    [Required]
    public int ProjectId { get; init; }

    [Required]
    public required UserProfileDto User { get; init; }

    [Required]
    public ProjectRole Role { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ProjectMember, ProjectMemberDto>();
        }
    }
}

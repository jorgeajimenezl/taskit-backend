using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Taskit.Domain.Entities;

namespace Taskit.Application.DTOs;

public record UpdateProjectRequest
{
    [StringLength(100)]
    public string? Name { get; init; }

    [StringLength(500)]
    public string? Description { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<UpdateProjectRequest, Project>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<ProjectDto, UpdateProjectRequest>();
        }
    }
}

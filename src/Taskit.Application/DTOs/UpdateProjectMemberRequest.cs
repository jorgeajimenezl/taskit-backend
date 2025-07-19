using AutoMapper;
using Taskit.Domain.Entities;
using Taskit.Domain.Enums;

namespace Taskit.Application.DTOs;

public record UpdateProjectMemberRequest
{
    public ProjectRole? Role { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<UpdateProjectMemberRequest, ProjectMember>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}

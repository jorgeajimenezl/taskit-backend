using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Taskit.Domain.Entities;

namespace Taskit.Application.DTOs;

public record UpdateTaskCommentRequest
{
    [Required, StringLength(1000)]
    public required string Content { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<UpdateTaskCommentRequest, TaskComment>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<TaskCommentDto, UpdateTaskCommentRequest>();
        }
    }
}

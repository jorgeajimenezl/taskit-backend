using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Taskit.Domain.Entities;

namespace Taskit.Application.DTOs;

public record TaskCommentDto
{
    public int Id { get; init; }

    [Required]
    public required string Content { get; init; }

    [Required]
    public required UserDto Author { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<TaskComment, TaskCommentDto>()
                .ForMember(d => d.Author, o => o.MapFrom(s => s.Author));
        }
    }
}

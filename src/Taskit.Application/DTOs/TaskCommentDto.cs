using AutoMapper;
using Taskit.Domain.Entities;

namespace Taskit.Application.DTOs;

public record TaskCommentDto
{
    public int Id { get; init; }
    public required string Content { get; init; }
    public required string AuthorId { get; init; }
    public required string AuthorUsername { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<TaskComment, TaskCommentDto>()
                .ForMember(d => d.AuthorUsername, o => o.MapFrom(s => s.Author!.UserName));
        }
    }
}

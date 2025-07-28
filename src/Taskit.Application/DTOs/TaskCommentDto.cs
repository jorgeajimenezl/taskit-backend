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
    public required string AuthorId { get; init; }

    [Required]
    public required string AuthorUsername { get; init; }

    public string? AuthorAvatarUrl { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<TaskComment, TaskCommentDto>()
                .ForMember(d => d.AuthorUsername, o => o.MapFrom(s => s.Author!.UserName))
                .ForMember(d => d.AuthorAvatarUrl,
                    o => o.MapFrom(s => s.Author!.Avatar != null && s.Author.Avatar.Media != null
                        ? $"/media/{s.Author.Avatar.Media.FileName}"
                        : null));
        }
    }
}

using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Taskit.Domain.Entities;

namespace Taskit.Application.DTOs;

public record CreateTaskCommentRequest
{
    [Required]
    [StringLength(500)]
    public required string Content { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<CreateTaskCommentRequest, TaskComment>();
        }
    }
}

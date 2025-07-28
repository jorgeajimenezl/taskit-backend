using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Taskit.Domain.Entities;

namespace Taskit.Application.DTOs;

public record TagDto
{
    public int Id { get; init; }

    [Required]
    public required string Name { get; init; }

    [Required]
    public required string Color { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<TaskTag, TagDto>();
        }
    }
}

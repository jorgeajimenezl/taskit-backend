using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Taskit.Domain.Entities;

namespace Taskit.Application.DTOs;

public record ProjectDto
{
    public int Id { get; init; }

    [Required]
    public required string Name { get; init; }

    [Required]
    public required string Description { get; init; }

    [Required]
    public required UserDto Owner { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Project, ProjectDto>();
        }
    }
}

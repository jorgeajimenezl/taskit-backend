using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Taskit.Domain.Entities;

namespace Taskit.Application.DTOs;

public record CreateProjectRequest
{
    [Required]
    [StringLength(100)]
    public required string Name { get; init; }

    [StringLength(500)]
    public string? Description { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<CreateProjectRequest, Project>();
        }
    }
}

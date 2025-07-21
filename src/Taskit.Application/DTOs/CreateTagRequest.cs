using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Taskit.Domain.Entities;

namespace Taskit.Application.DTOs;

public record CreateTagRequest
{
    [Required]
    [StringLength(50)]
    public required string Name { get; init; }

    [Required]
    [StringLength(7)]
    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "The color must be a valid hex code in the format #RRGGBB.")]
    public string Color { get; init; } = "#000000";

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<CreateTagRequest, TaskTag>();
        }
    }
}

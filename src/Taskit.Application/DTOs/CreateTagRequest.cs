using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Taskit.Domain.Entities;

namespace Taskit.Application.DTOs;

public record CreateTagRequest
{
    [Required]
    public required string Name { get; init; }

    public string Color { get; init; } = "#000000";

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<CreateTagRequest, TaskTag>();
        }
    }
}

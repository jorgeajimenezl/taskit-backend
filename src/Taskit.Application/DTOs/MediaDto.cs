using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Taskit.Domain.Entities;

namespace Taskit.Application.DTOs;

public record MediaDto
{
    public int Id { get; init; }

    [Required]
    public required string Url { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Media, MediaDto>()
                .ForMember(d => d.Url, opt => opt.MapFrom(src => $"/media/{src.Id}"));
        }
    }
}

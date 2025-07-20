using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.IO;
using AutoMapper;
using Taskit.Application.DTOs;
using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;

namespace Taskit.Application.Services;

public class MediaService(IMediaRepository mediaRepository, IWebHostEnvironment environment, IMapper mapper)
{
    private readonly IMediaRepository _mediaRepository = mediaRepository;
    private readonly IWebHostEnvironment _environment = environment;
    private readonly IMapper _mapper = mapper;

    private string UploadsPath => Path.Combine(
        _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
        "uploads");

    public Task AddAsync(Media media)
    {
        return _mediaRepository.AddAsync(media);
    }

    public async Task<MediaDto?> GetByIdAsync(int id)
    {
        var media = await _mediaRepository.GetByIdAsync(id);
        return media is null ? null : _mapper.Map<MediaDto>(media);
    }

    public async Task<MediaDto> UploadAsync(IFormFile file, string userId)
    {
        Directory.CreateDirectory(UploadsPath);
        var extension = Path.GetExtension(file.FileName);
        var storedName = $"{Guid.NewGuid()}{extension}";
        var path = Path.Combine(UploadsPath, storedName);

        using (var stream = File.Create(path))
        {
            await file.CopyToAsync(stream);
        }

        var media = new Media
        {
            Uuid = Guid.NewGuid(),
            CollectionName = "default",
            Name = file.FileName,
            FileName = storedName,
            MimeType = file.ContentType,
            Disk = "local",
            Size = (ulong)file.Length,
            ModelId = 0,
            ModelType = "Generic",
            UploadedById = userId
        };

        await _mediaRepository.AddAsync(media);
        return _mapper.Map<MediaDto>(media);
    }

    public async Task<IEnumerable<Media>> GetMediaAsync(string modelType, int modelId, string? collectionName = null)
    {
        var query = _mediaRepository.Query()
            .Where(m => m.ModelType == modelType && m.ModelId == modelId);
        if (!string.IsNullOrEmpty(collectionName))
            query = query.Where(m => m.CollectionName == collectionName);
        return await query.ToListAsync();
    }

    public async Task<Media?> GetFirstMediaAsync(string modelType, int modelId, string? collectionName = null)
    {
        var query = _mediaRepository.Query()
            .Where(m => m.ModelType == modelType && m.ModelId == modelId);
        if (!string.IsNullOrEmpty(collectionName))
            query = query.Where(m => m.CollectionName == collectionName);
        return await query.FirstOrDefaultAsync();
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var media = await _mediaRepository.GetByIdAsync(id);
        if (media is null)
            return false;
        if (media.UploadedById != userId && media.ModelId != 0)
            return false;

        var path = Path.Combine(UploadsPath, media.FileName);
        if (File.Exists(path))
            File.Delete(path);

        await _mediaRepository.DeleteAsync(id);
        return true;
    }

    public async Task ClearMediaCollectionAsync(string modelType, int modelId, string collectionName)
    {
        var mediaItems = await _mediaRepository.Query()
            .Where(m => m.ModelType == modelType &&
                        m.ModelId == modelId &&
                        m.CollectionName == collectionName)
            .ToListAsync();

        if (mediaItems.Any())
            await _mediaRepository.DeleteRangeAsync(mediaItems);
    }

    public Task ClearMediaCollectionAsync<TModel>(int modelId, string collectionName)
        where TModel : class
    {
        return ClearMediaCollectionAsync(typeof(TModel).Name, modelId, collectionName);
    }
}
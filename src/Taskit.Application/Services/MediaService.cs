using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Collections.Generic;
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

    private static readonly string[] _allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf" };
    private static readonly HashSet<string> _allowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/gif", "application/pdf"
    };
    private const long _maxFileSize = 10 * 1024 * 1024; // 10 MB

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

    public async Task<MediaDto?> UploadAsync(
        IFormFile file,
        string userId,
        int? modelId = null,
        string? modelType = null,
        string? collectionName = null
    )
    {
        if (!IsValidFile(file))
            return null;

        try
        {
            Directory.CreateDirectory(UploadsPath);
            var extension = Path.GetExtension(file.FileName);
            var storedName = $"{Guid.NewGuid()}{extension}";
            var path = Path.Combine(UploadsPath, storedName);

            using (var stream = File.Create(path))
            {
                await file.CopyToAsync(stream);
            }
        }
        catch (IOException ex)
        {
            // Log the exception or handle it appropriately
            Console.WriteLine($"File operation failed: {ex.Message}");
            return null;
        }
        catch (UnauthorizedAccessException ex)
        {
            // Log the exception or handle it appropriately
            Console.WriteLine($"Access denied: {ex.Message}");
            return null;
        }
        catch (DirectoryNotFoundException ex)
        {
            // Log the exception or handle it appropriately
            Console.WriteLine($"Directory not found: {ex.Message}");
            return null;
        }

        var media = new Media
        {
            Uuid = Guid.NewGuid(),
            CollectionName = collectionName ?? "default",
            Name = file.FileName,
            FileName = storedName,
            MimeType = file.ContentType,
            Disk = "local",
            Size = (ulong)file.Length,
            ModelId = modelId,
            ModelType = modelType,
            UploadedById = userId
        };

        await _mediaRepository.AddAsync(media);
        return _mapper.Map<MediaDto>(media);
    }

    private static bool IsValidFile(IFormFile file)
    {
        if (file.Length == 0 || file.Length > _maxFileSize)
            return false;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !_allowedExtensions.Contains(extension))
            return false;

        if (string.IsNullOrEmpty(file.ContentType) || !_allowedMimeTypes.Contains(file.ContentType))
            return false;

        return true;
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
        if (media.UploadedById != userId)
            return false;

        var sanitizedFileName = Path.GetFileName(media.FileName);
        var path = Path.Combine(UploadsPath, sanitizedFileName);
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


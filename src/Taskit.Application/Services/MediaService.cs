using Microsoft.EntityFrameworkCore;
using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;

namespace Taskit.Application.Services;

public class MediaService(IMediaRepository mediaRepository)
{
    private readonly IMediaRepository _mediaRepository = mediaRepository;

    public Task AddAsync(Media media)
    {
        return _mediaRepository.AddAsync(media);
    }

    public Task<Media?> GetByIdAsync(int id)
    {
        return _mediaRepository.GetByIdAsync(id);
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

    public async Task<bool> DeleteAsync(int id)
    {
        if (!await _mediaRepository.ExistsAsync(id))
            return false;

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
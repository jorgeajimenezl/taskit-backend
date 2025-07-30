using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using MockQueryable.Moq;
using Moq;
using Taskit.Application.DTOs;
using Taskit.Application.Interfaces;
using Taskit.Application.Services;
using Taskit.Application.Common.Exceptions;
using Taskit.Domain.Entities;
using Taskit.Domain.Enums;
using MassTransit;
using Xunit;

namespace Taskit.Application.Tests.Services;

public class MediaServiceTests
{
    private static IMapper CreateMapper()
    {
        var mapper = new Mock<IMapper>();
        mapper.Setup(m => m.Map<MediaDto>(It.IsAny<Media>()))
            .Returns<Media>(m => new MediaDto { Id = m.Id, Url = $"/media/{m.Id}" });
        mapper.Setup(m => m.Map<IEnumerable<MediaDto>>(It.IsAny<IEnumerable<Media>>()))
            .Returns<IEnumerable<Media>>(list => list.Select(m => new MediaDto { Id = m.Id, Url = $"/media/{m.Id}" }).ToList());
        return mapper.Object;
    }

    private static ProjectActivityLogService CreateActivityService()
    {
        var repo = new Mock<IProjectActivityLogRepository>();
        repo.Setup(r => r.AddAsync(It.IsAny<ProjectActivityLog>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        var publisher = new Mock<IPublishEndpoint>();
        publisher.Setup(p => p.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new ProjectActivityLogService(repo.Object, CreateMapper(), publisher.Object);
    }

    private static MediaService CreateService(Mock<IMediaRepository> repo, string rootPath)
    {
        var env = new Mock<IWebHostEnvironment>();
        env.SetupGet(e => e.WebRootPath).Returns(rootPath);
        var tasks = new Mock<ITaskRepository>();
        tasks.Setup(t => t.QueryForUser(It.IsAny<string>()))
            .Returns(new List<AppTask>().AsQueryable().BuildMock());
        return new MediaService(repo.Object, env.Object, CreateMapper(), CreateActivityService(), tasks.Object);
    }

    private static IFormFile CreateFormFile(string name, string contentType, byte[] content)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "file", name)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    [Fact]
    public async Task AddAsync_CallsRepository()
    {
        var repo = new Mock<IMediaRepository>();
        repo.Setup(r => r.AddAsync(It.IsAny<Media>(), It.IsAny<bool>())).Returns(Task.CompletedTask).Verifiable();
        var service = CreateService(repo, Path.GetTempPath());
        var media = new Media { FileName = "f.jpg", Name = "f.jpg", Disk = "local", CollectionName = "c", Uuid = Guid.NewGuid(), AccessScope = AccessScope.Private };

        await service.AddAsync(media);

        repo.Verify(r => r.AddAsync(media, It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDto()
    {
        var repo = new Mock<IMediaRepository>();
        var media = new Media { Id = 1, FileName = "f.jpg", Name = "f.jpg", Disk = "local", CollectionName = "c", Uuid = Guid.NewGuid(), AccessScope = AccessScope.Private };
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(media);
        var service = CreateService(repo, Path.GetTempPath());

        var result = await service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal($"/media/{media.Id}", result!.Url);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        var repo = new Mock<IMediaRepository>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Media?)null);
        var service = CreateService(repo, Path.GetTempPath());

        var result = await service.GetByIdAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task UploadAsync_InvalidFile_ThrowsValidationException()
    {
        var repo = new Mock<IMediaRepository>();
        var service = CreateService(repo, Path.GetTempPath());
        var file = CreateFormFile("bad.exe", "application/octet-stream", Encoding.UTF8.GetBytes("bad"));

        await Assert.ThrowsAsync<ValidationException>(() => service.UploadAsync(file, "u"));
    }

    [Fact]
    public async Task UploadAsync_ValidFile_SavesMedia()
    {
        var temp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(temp);
        try
        {
            var repo = new Mock<IMediaRepository>();
            Media? saved = null;
            repo.Setup(r => r.AddAsync(It.IsAny<Media>(), It.IsAny<bool>()))
                .Callback<Media, bool>((m, _) => saved = m)
                .Returns(Task.CompletedTask);
            var service = CreateService(repo, temp);
            var file = CreateFormFile("img.jpg", "image/jpeg", Encoding.UTF8.GetBytes("img"));

            var dto = await service.UploadAsync(file, "u", "2", nameof(AppTask), "default");

            Assert.NotNull(saved);
            var path = Path.Combine(temp, "uploads", saved!.FileName);
            Assert.True(File.Exists(path));
            Assert.Equal($"/media/{saved.Id}", dto.Url);
        }
        finally
        {
            Directory.Delete(temp, true);
        }
    }

    [Fact]
    public async Task GetMediaAsync_ReturnsFiltered()
    {
        var repo = new Mock<IMediaRepository>();
        var items = new List<Media>
        {
            new() { Id = 1, ModelType = nameof(AppTask), ModelId = "2", CollectionName = "a", FileName = "a.jpg", Name="a", Disk="d", Uuid=Guid.NewGuid(), AccessScope = AccessScope.Private },
            new() { Id = 2, ModelType = nameof(Project), ModelId = "2", CollectionName = "a", FileName = "b.jpg", Name="b", Disk="d", Uuid=Guid.NewGuid(), AccessScope = AccessScope.Private }
        };
        repo.Setup(r => r.Query()).Returns(items.AsQueryable().BuildMock());
        var service = CreateService(repo, Path.GetTempPath());

        var result = await service.GetMediaAsync(nameof(AppTask), "2", "a");

        Assert.Single(result);
        Assert.Equal(1, result.First().Id);
    }

    [Fact]
    public async Task GetFirstMediaAsync_ReturnsFirst()
    {
        var repo = new Mock<IMediaRepository>();
        var items = new List<Media>
        {
            new() { Id = 1, ModelType = nameof(AppTask), ModelId = "1", CollectionName = "a", FileName = "a.jpg", Name="a", Disk="d", Uuid=Guid.NewGuid(), AccessScope = AccessScope.Private },
            new() { Id = 2, ModelType = nameof(AppTask), ModelId = "1", CollectionName = "b", FileName = "b.jpg", Name="b", Disk="d", Uuid=Guid.NewGuid(), AccessScope = AccessScope.Private }
        };
        repo.Setup(r => r.Query()).Returns(items.AsQueryable().BuildMock());
        var service = CreateService(repo, Path.GetTempPath());

        var result = await service.GetFirstMediaAsync(nameof(AppTask), "1");

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
    }

    [Fact]
    public async Task DeleteAsync_RemovesFileAndEntity()
    {
        var temp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(Path.Combine(temp, "uploads"));
        var stored = "file.jpg";
        var fullPath = Path.Combine(temp, "uploads", stored);
        await File.WriteAllTextAsync(fullPath, "x");
        try
        {
            var repo = new Mock<IMediaRepository>();
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Media
            {
                Id = 1,
                FileName = stored,
                UploadedById = "u",
                CollectionName = "c",
                ModelType = nameof(AppTask),
                ModelId = "2",
                Name = stored,
                Disk = "d",
                Uuid = Guid.NewGuid(),
                AccessScope = AccessScope.Private
            });
            repo.Setup(r => r.DeleteAsync(1, It.IsAny<bool>())).Returns(Task.CompletedTask).Verifiable();
            var service = CreateService(repo, temp);

            await service.DeleteAsync(1, "u");

            Assert.False(File.Exists(fullPath));
            repo.Verify(r => r.DeleteAsync(1, It.IsAny<bool>()), Times.Once);
        }
        finally
        {
            Directory.Delete(temp, true);
        }
    }

    [Fact]
    public async Task DeleteAsync_WrongUser_ThrowsForbidden()
    {
        var repo = new Mock<IMediaRepository>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Media { Id = 1, FileName = "f.jpg", UploadedById = "other", CollectionName = "c", Name = "f.jpg", Disk = "d", Uuid = Guid.NewGuid(), AccessScope = AccessScope.Private });
        var service = CreateService(repo, Path.GetTempPath());

        await Assert.ThrowsAsync<ForbiddenAccessException>(() => service.DeleteAsync(1, "u"));
    }

    [Fact]
    public async Task ClearMediaCollectionAsync_DeletesRange()
    {
        var repo = new Mock<IMediaRepository>();
        var items = new List<Media> { new() { Id = 1, ModelType = nameof(AppTask), ModelId = "1", CollectionName = "c", FileName = "a.jpg", Name = "a", Disk = "d", Uuid = Guid.NewGuid(), AccessScope = AccessScope.Private } };
        repo.Setup(r => r.Query()).Returns(items.AsQueryable().BuildMock());
        repo.Setup(r => r.DeleteRangeAsync(It.IsAny<IEnumerable<Media>>(), It.IsAny<bool>())).Returns(Task.CompletedTask).Verifiable();
        var service = CreateService(repo, Path.GetTempPath());

        await service.ClearMediaCollectionAsync(nameof(AppTask), "1", "c");

        repo.Verify(r => r.DeleteRangeAsync(It.Is<IEnumerable<Media>>(m => m.Count() == 1), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task ClearMediaCollection_Generic_DelegatesCall()
    {
        var repo = new Mock<IMediaRepository>();
        var items = new List<Media> { new() { Id = 1, ModelType = nameof(Project), ModelId = "1", CollectionName = "c", FileName = "a.jpg", Name = "a", Disk = "d", Uuid = Guid.NewGuid(), AccessScope = AccessScope.Private } };
        repo.Setup(r => r.Query()).Returns(items.AsQueryable().BuildMock());
        repo.Setup(r => r.DeleteRangeAsync(It.IsAny<IEnumerable<Media>>(), It.IsAny<bool>())).Returns(Task.CompletedTask).Verifiable();
        var service = CreateService(repo, Path.GetTempPath());

        await service.ClearMediaCollectionAsync<Project>("1", "c");

        repo.Verify(r => r.DeleteRangeAsync(It.IsAny<IEnumerable<Media>>(), It.IsAny<bool>()), Times.Once);
    }
}

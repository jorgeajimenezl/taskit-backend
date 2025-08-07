using AutoMapper;
using MockQueryable.Moq;
using Moq;
using Taskit.Application.DTOs;
using Taskit.Application.Interfaces;
using Taskit.Application.Services;
using Taskit.Domain.Entities;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Taskit.Application.Tests.Services;

public class TagServiceTests
{
    private static IMapper CreateMapper()
    {
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<TagService>>();
        var mockFactory = new Mock<Microsoft.Extensions.Logging.ILoggerFactory>();
        mockFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<TaskTag, TagDto>();
            cfg.CreateMap<CreateTagRequest, TaskTag>();
        }, mockFactory.Object);
        return config.CreateMapper();
    }

    private static TagService CreateService(Mock<ITagRepository> repo, IMapper mapper)
    {
        return new TagService(repo.Object, mapper);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllTags()
    {
        var mapper = CreateMapper();
        var tags = new List<TaskTag>
        {
            new() { Id = 1, Name = "t1", Color = "#000001" },
            new() { Id = 2, Name = "t2", Color = "#000002" }
        };
        var repo = new Mock<ITagRepository>();
        repo.Setup(r => r.Query()).Returns(tags.AsQueryable().BuildMock());
        var service = CreateService(repo, mapper);

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count());
        Assert.Equal("t1", result.First().Name);
        repo.Verify(r => r.Query(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_AddsTagAndReturnsDto()
    {
        var mapper = CreateMapper();
        var repo = new Mock<ITagRepository>();
        repo.Setup(r => r.AddAsync(It.IsAny<TaskTag>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();
        var service = CreateService(repo, mapper);
        var request = new CreateTagRequest { Name = "new", Color = "#FFAA00" };

        var result = await service.CreateAsync(request);

        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Color, result.Color);
        repo.Verify(r => r.AddAsync(It.Is<TaskTag>(t => t.Name == request.Name && t.Color == request.Color), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}

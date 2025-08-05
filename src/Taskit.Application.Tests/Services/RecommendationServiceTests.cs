using AutoMapper;
using MassTransit;
using MockQueryable.Moq;
using Moq;
using Microsoft.Extensions.Logging;
using Taskit.Application.DTOs;
using Taskit.Application.Interfaces;
using Taskit.Application.Services;
using Taskit.Domain.Entities;
using Taskit.Domain.Messages;

namespace Taskit.Application.Tests.Services;

public class RecommendationServiceTests
{
    private static IMapper CreateMapper()
    {
        var mockLogger = new Mock<ILogger<RecommendationServiceTests>>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        mockLoggerFactory
            .Setup(f => f.CreateLogger(It.IsAny<string>()))
            .Returns(mockLogger.Object);

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<AppTask, TaskDto>()
                .ForMember(d => d.ProjectName, o => o.MapFrom(s => s.Project!.Name))
                .ForMember(d => d.ProjectId, o => o.MapFrom(s => s.ProjectId));
            cfg.CreateMap<AppUser, UserDto>();
            cfg.CreateMap<TaskTag, TagDto>();
        }, mockLoggerFactory.Object);
        return config.CreateMapper();
    }

    [Fact]
    public async Task GetRelatedTasksAsync_ReturnsProcessing_WhenEmbeddingsPending()
    {
        var repo = new Mock<ITaskRepository>();
        var client = new Mock<IRequestClient<RelatedTasksQuery>>();
        var response = Mock.Of<Response<OperationResult<RelatedTasksQueryResult>>>(r => r.Message == OperationResult<RelatedTasksQueryResult>.Processing());
        client.Setup(c => c.GetResponse<OperationResult<RelatedTasksQueryResult>>(It.IsAny<RelatedTasksQuery>(), default, default))
            .ReturnsAsync(response);
        var service = new RecommendationService(repo.Object, client.Object, CreateMapper());

        var result = await service.GetRelatedTasksAsync(1, "u");

        Assert.True(result.IsProcessing);
        Assert.Empty(result.Tasks);
    }

    [Fact]
    public async Task GetRelatedTasksAsync_ReturnsTasks_WhenAvailable()
    {
        var tasks = new List<AppTask> { new() { Id = 2, Title = "T", Description = "D", ProjectId = 1, Project = new Project { Id = 1, Name = "P", OwnerId = "u" } } };
        var repo = new Mock<ITaskRepository>();
        repo.Setup(r => r.QueryForUser("u")).Returns(tasks.AsQueryable().BuildMock());

        var client = new Mock<IRequestClient<RelatedTasksQuery>>();
        var response = Mock.Of<Response<OperationResult<RelatedTasksQueryResult>>>(r => r.Message == OperationResult<RelatedTasksQueryResult>.Success(new(new List<int> { 2 })));
        client.Setup(c => c.GetResponse<OperationResult<RelatedTasksQueryResult>>(It.IsAny<RelatedTasksQuery>(), default, default))
            .ReturnsAsync(response);

        var service = new RecommendationService(repo.Object, client.Object, CreateMapper());

        var result = await service.GetRelatedTasksAsync(1, "u");

        Assert.False(result.IsProcessing);
        Assert.Single(result.Tasks);
        Assert.Equal(2, result.Tasks.First().Id);
    }
}

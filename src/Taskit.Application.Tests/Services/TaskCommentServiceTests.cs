using AutoMapper;
using Ardalis.GuardClauses;
using MockQueryable.Moq;
using Moq;
using Taskit.Application.DTOs;
using Taskit.Application.Interfaces;
using Taskit.Application.Common.Exceptions;
using Taskit.Application.Services;
using Taskit.Domain.Entities;
using Taskit.Domain.Enums;
using Microsoft.Extensions.Logging;
using MassTransit;
using Xunit;

namespace Taskit.Application.Tests.Services;

public class TaskCommentServiceTests
{
    private static IMapper CreateMapper()
    {
        var mockLogger = new Mock<ILogger<TaskCommentService>>();
        var mockFactory = new Mock<ILoggerFactory>();
        mockFactory
            .Setup(f => f.CreateLogger(It.IsAny<string>()))
            .Returns(mockLogger.Object);

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<TaskComment, TaskCommentDto>()
                .ForMember(d => d.AuthorUsername, o => o.MapFrom(s => s.Author!.UserName));
            cfg.CreateMap<CreateTaskCommentRequest, TaskComment>();
            cfg.CreateMap<UpdateTaskCommentRequest, TaskComment>()
                .ForAllMembers(o => o.Condition((src, dest, member) => member != null));
        }, mockFactory.Object);
        return config.CreateMapper();
    }

    private static ProjectActivityLogService CreateActivityService(IMapper mapper)
    {
        var repo = new Mock<IProjectActivityLogRepository>();
        repo.Setup(r => r.AddAsync(It.IsAny<ProjectActivityLog>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        var publisher = new Mock<IPublishEndpoint>();
        return new ProjectActivityLogService(repo.Object, mapper, publisher.Object);
    }

    private static TaskCommentService CreateService(
        Mock<ITaskCommentRepository> comments,
        Mock<ITaskRepository> tasks,
        IMapper mapper)
    {
        var activity = CreateActivityService(mapper);
        return new TaskCommentService(comments.Object, tasks.Object, mapper, activity);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsComments()
    {
        var mapper = CreateMapper();
        var taskRepo = new Mock<ITaskRepository>();
        taskRepo.Setup(r => r.QueryForUser("u"))
            .Returns(new List<AppTask> { new() { Id = 1, ProjectId = 1, Title = "T" } }.AsQueryable().BuildMock());
        var comments = new List<TaskComment>
        {
            new() { Id = 1, Content = "C", TaskId = 1, AuthorId = "u", Author = new AppUser { Id = "u", UserName = "user" } }
        };
        var commentRepo = new Mock<ITaskCommentRepository>();
        commentRepo.Setup(r => r.QueryForTask(1)).Returns(comments.AsQueryable().BuildMock());
        var service = CreateService(commentRepo, taskRepo, mapper);

        var result = await service.GetAllAsync(1, "u");

        Assert.Single(result);
        Assert.Equal(1, result.First().Id);
    }

    [Fact]
    public async Task GetAllAsync_UserWithoutAccess_ThrowsForbidden()
    {
        var mapper = CreateMapper();
        var taskRepo = new Mock<ITaskRepository>();
        taskRepo.Setup(r => r.QueryForUser("u"))
            .Returns(new List<AppTask>().AsQueryable().BuildMock());
        var commentRepo = new Mock<ITaskCommentRepository>();
        var service = CreateService(commentRepo, taskRepo, mapper);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() => service.GetAllAsync(1, "u"));
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsComment()
    {
        var mapper = CreateMapper();
        var taskRepo = new Mock<ITaskRepository>();
        taskRepo.Setup(r => r.QueryForUser("u"))
            .Returns(new List<AppTask> { new() { Id = 1, ProjectId = 1, Title = "T" } }.AsQueryable().BuildMock());
        var comments = new List<TaskComment>
        {
            new() { Id = 1, Content = "C", TaskId = 1, AuthorId = "u", Author = new AppUser { Id = "u", UserName = "user" } }
        };
        var commentRepo = new Mock<ITaskCommentRepository>();
        commentRepo.Setup(r => r.QueryForTask(1)).Returns(comments.AsQueryable().BuildMock());
        var service = CreateService(commentRepo, taskRepo, mapper);

        var result = await service.GetByIdAsync(1, 1, "u");

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_CommentNotFound_ThrowsNotFound()
    {
        var mapper = CreateMapper();
        var taskRepo = new Mock<ITaskRepository>();
        taskRepo.Setup(r => r.QueryForUser("u"))
            .Returns(new List<AppTask> { new() { Id = 1, ProjectId = 1, Title = "T" } }.AsQueryable().BuildMock());
        var commentRepo = new Mock<ITaskCommentRepository>();
        commentRepo.Setup(r => r.QueryForTask(1)).Returns(new List<TaskComment>().AsQueryable().BuildMock());
        var service = CreateService(commentRepo, taskRepo, mapper);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(1, 1, "u"));
    }

    [Fact]
    public async Task GetByIdAsync_UserWithoutAccess_ThrowsForbidden()
    {
        var mapper = CreateMapper();
        var taskRepo = new Mock<ITaskRepository>();
        taskRepo.Setup(r => r.QueryForUser("u"))
            .Returns(new List<AppTask>().AsQueryable().BuildMock());
        var commentRepo = new Mock<ITaskCommentRepository>();
        var service = CreateService(commentRepo, taskRepo, mapper);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() => service.GetByIdAsync(1, 1, "u"));
    }

    [Fact]
    public async Task CreateAsync_AddsComment()
    {
        var mapper = CreateMapper();
        var task = new AppTask { Id = 1, ProjectId = 1, Title = "T" };
        var taskRepo = new Mock<ITaskRepository>();
        taskRepo.Setup(r => r.QueryForUser("u"))
            .Returns(new List<AppTask> { task }.AsQueryable().BuildMock());
        taskRepo.Setup(r => r.Query())
            .Returns(new List<AppTask> { task }.AsQueryable().BuildMock());
        var commentRepo = new Mock<ITaskCommentRepository>();
        commentRepo.Setup(r => r.AddAsync(It.IsAny<TaskComment>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask)
            .Verifiable();
        var service = CreateService(commentRepo, taskRepo, mapper);

        var dto = new CreateTaskCommentRequest { Content = "C" };
        var result = await service.CreateAsync(1, dto, "u");

        commentRepo.Verify(r => r.AddAsync(It.Is<TaskComment>(c => c.TaskId == 1 && c.AuthorId == "u"), It.IsAny<bool>()), Times.Once);
        Assert.Equal("C", result.Content);
    }

    [Fact]
    public async Task CreateAsync_UserWithoutAccess_ThrowsForbidden()
    {
        var mapper = CreateMapper();
        var taskRepo = new Mock<ITaskRepository>();
        taskRepo.Setup(r => r.QueryForUser("u"))
            .Returns(new List<AppTask>().AsQueryable().BuildMock());
        var commentRepo = new Mock<ITaskCommentRepository>();
        var service = CreateService(commentRepo, taskRepo, mapper);

        var dto = new CreateTaskCommentRequest { Content = "C" };
        await Assert.ThrowsAsync<ForbiddenAccessException>(() => service.CreateAsync(1, dto, "u"));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesComment()
    {
        var mapper = CreateMapper();
        var taskRepo = new Mock<ITaskRepository>();
        taskRepo.Setup(r => r.QueryForUser("u"))
            .Returns(new List<AppTask> { new() { Id = 1, ProjectId = 1, Title = "T" } }.AsQueryable().BuildMock());
        var comment = new TaskComment { Id = 1, TaskId = 1, AuthorId = "u", Content = "Old" };
        var commentRepo = new Mock<ITaskCommentRepository>();
        commentRepo.Setup(r => r.QueryForTask(1)).Returns(new List<TaskComment> { comment }.AsQueryable().BuildMock());
        commentRepo.Setup(r => r.UpdateAsync(comment, It.IsAny<bool>())).Returns(Task.CompletedTask).Verifiable();
        var service = CreateService(commentRepo, taskRepo, mapper);

        var dto = new UpdateTaskCommentRequest { Content = "New" };
        await service.UpdateAsync(1, 1, dto, "u");

        Assert.Equal("New", comment.Content);
        commentRepo.Verify(r => r.UpdateAsync(comment, It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_CommentNotFound_ThrowsNotFound()
    {
        var mapper = CreateMapper();
        var taskRepo = new Mock<ITaskRepository>();
        taskRepo.Setup(r => r.QueryForUser("u"))
            .Returns(new List<AppTask> { new() { Id = 1, ProjectId = 1, Title = "T" } }.AsQueryable().BuildMock());
        var commentRepo = new Mock<ITaskCommentRepository>();
        commentRepo.Setup(r => r.QueryForTask(1)).Returns(new List<TaskComment>().AsQueryable().BuildMock());
        var service = CreateService(commentRepo, taskRepo, mapper);

        var dto = new UpdateTaskCommentRequest { Content = "New" };
        await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateAsync(1, 1, dto, "u"));
    }

    [Fact]
    public async Task UpdateAsync_NotAuthor_ThrowsForbidden()
    {
        var mapper = CreateMapper();
        var taskRepo = new Mock<ITaskRepository>();
        taskRepo.Setup(r => r.QueryForUser("u"))
            .Returns(new List<AppTask> { new() { Id = 1, ProjectId = 1, Title = "T" } }.AsQueryable().BuildMock());
        var comment = new TaskComment { Id = 1, TaskId = 1, AuthorId = "other", Content = "Old" };
        var commentRepo = new Mock<ITaskCommentRepository>();
        commentRepo.Setup(r => r.QueryForTask(1)).Returns(new List<TaskComment> { comment }.AsQueryable().BuildMock());
        var service = CreateService(commentRepo, taskRepo, mapper);

        var dto = new UpdateTaskCommentRequest { Content = "New" };
        await Assert.ThrowsAsync<ForbiddenAccessException>(() => service.UpdateAsync(1, 1, dto, "u"));
    }

    [Fact]
    public async Task DeleteAsync_DeletesComment()
    {
        var mapper = CreateMapper();
        var taskRepo = new Mock<ITaskRepository>();
        taskRepo.Setup(r => r.QueryForUser("u"))
            .Returns(new List<AppTask> { new() { Id = 1, ProjectId = 1, Title = "T" } }.AsQueryable().BuildMock());
        var comment = new TaskComment { Id = 1, TaskId = 1, AuthorId = "u", Content = "C" };
        var commentRepo = new Mock<ITaskCommentRepository>();
        commentRepo.Setup(r => r.QueryForTask(1)).Returns(new List<TaskComment> { comment }.AsQueryable().BuildMock());
        commentRepo.Setup(r => r.DeleteAsync(1, It.IsAny<bool>())).Returns(Task.CompletedTask).Verifiable();
        var service = CreateService(commentRepo, taskRepo, mapper);

        await service.DeleteAsync(1, 1, "u");

        commentRepo.Verify(r => r.DeleteAsync(1, It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CommentNotFound_ThrowsNotFound()
    {
        var mapper = CreateMapper();
        var taskRepo = new Mock<ITaskRepository>();
        taskRepo.Setup(r => r.QueryForUser("u"))
            .Returns(new List<AppTask> { new() { Id = 1, ProjectId = 1, Title = "T" } }.AsQueryable().BuildMock());
        var commentRepo = new Mock<ITaskCommentRepository>();
        commentRepo.Setup(r => r.QueryForTask(1)).Returns(new List<TaskComment>().AsQueryable().BuildMock());
        var service = CreateService(commentRepo, taskRepo, mapper);

        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(1, 1, "u"));
    }

    [Fact]
    public async Task DeleteAsync_NotAuthor_ThrowsForbidden()
    {
        var mapper = CreateMapper();
        var taskRepo = new Mock<ITaskRepository>();
        taskRepo.Setup(r => r.QueryForUser("u"))
            .Returns(new List<AppTask> { new() { Id = 1, ProjectId = 1, Title = "T" } }.AsQueryable().BuildMock());
        var comment = new TaskComment { Id = 1, TaskId = 1, AuthorId = "other", Content = "C" };
        var commentRepo = new Mock<ITaskCommentRepository>();
        commentRepo.Setup(r => r.QueryForTask(1)).Returns(new List<TaskComment> { comment }.AsQueryable().BuildMock());
        var service = CreateService(commentRepo, taskRepo, mapper);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() => service.DeleteAsync(1, 1, "u"));
    }
}

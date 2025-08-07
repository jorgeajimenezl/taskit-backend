using System.Collections.Generic;
using AutoMapper;
using Gridify;
using MockQueryable.Moq;
using Moq;
using Taskit.Application.DTOs;
using Taskit.Application.Interfaces;
using Taskit.Application.Services;
using Taskit.Domain.Entities;
using Taskit.Domain.Enums;
using MassTransit;
using TaskStatusEnum = Taskit.Domain.Enums.TaskStatus;
using Xunit;
using Microsoft.Extensions.Logging;
using Taskit.Application.Common.Exceptions;

namespace Taskit.Application.Tests.Services;

public class TaskServiceTests
{
    private static IMapper CreateMapper()
    {
        var mockLogger = new Mock<ILogger<TaskService>>();

        var mockLoggerFactory = new Mock<ILoggerFactory>();
        mockLoggerFactory
            .Setup(factory => factory.CreateLogger(It.IsAny<string>()))
            .Returns(mockLogger.Object);


        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<AppTask, TaskDto>();
            cfg.CreateMap<CreateTaskRequest, AppTask>();
            cfg.CreateMap<UpdateTaskRequest, AppTask>()
                .ForAllMembers(o => o.Condition((src, dest, member) => member != null));
            cfg.CreateMap<Media, MediaDto>()
                .ForMember(d => d.Url, opt => opt.MapFrom(s => $"http://localhost:5152/api/media/{s.Id}"));
            cfg.CreateMap<TaskTag, TagDto>();
            cfg.CreateMap<AppUser, UserDto>();
        }, mockLoggerFactory.Object);
        return config.CreateMapper();
    }

    private static ProjectActivityLogService CreateActivityService(Mock<IProjectActivityLogRepository> repo)
    {
        var mapper = CreateMapper();
        var publisher = new Mock<IPublishEndpoint>();
        repo.Setup(r => r.AddAsync(It.IsAny<ProjectActivityLog>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return new ProjectActivityLogService(repo.Object, mapper, publisher.Object);
    }

    private static TaskService CreateService(
        Mock<ITaskRepository> tasks,
        Mock<IProjectRepository> projects,
        Mock<ITagRepository> tags,
        Mock<IMediaRepository> media,
        ProjectActivityLogService activity,
        IMapper mapper)
    {
        return new TaskService(tasks.Object, projects.Object, tags.Object, media.Object, activity, mapper);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsTask()
    {
        var mapper = CreateMapper();
        var tasks = new List<AppTask> { new() {
            Id = 1, Title = "T", Description = "D", ProjectId = 1,
            Project = new Project() { Id = 1, Name = "P", OwnerId = "u"
        } } };
        var queryable = tasks.AsQueryable().BuildMock();
        var repo = new Mock<ITaskRepository>();
        repo.Setup(r => r.QueryForUser("u")).Returns(queryable);
        var service = CreateService(repo, new Mock<IProjectRepository>(), new Mock<ITagRepository>(), new Mock<IMediaRepository>(),
            CreateActivityService(new Mock<IProjectActivityLogRepository>()), mapper);

        var result = await service.GetByIdAsync(1, "u");

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
    }

    [Fact]
    public async Task CreateAsync_SavesTask()
    {
        var mapper = CreateMapper();
        var taskRepo = new Mock<ITaskRepository>();
        taskRepo.Setup(r => r.AddAsync(It.IsAny<AppTask>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var projRepo = new Mock<IProjectRepository>();
        var project = new Project { Id = 1, Name = "P", OwnerId = "u" };
        projRepo.Setup(p => p.Query()).Returns(new List<Project> { project }.AsQueryable().BuildMock());
        var service = CreateService(taskRepo, projRepo, new Mock<ITagRepository>(), new Mock<IMediaRepository>(),
            CreateActivityService(new Mock<IProjectActivityLogRepository>()), mapper);

        var dto = new CreateTaskRequest { Title = "T", ProjectId = 1 };
        var result = await service.CreateAsync(dto, "u");

        taskRepo.Verify(r => r.AddAsync(It.Is<AppTask>(t => t.Title == "T" && t.AuthorId == "u"), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal("T", result.Title);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesTask()
    {
        var mapper = CreateMapper();
        var task = new AppTask { Id = 1, Title = "Old", Description = "D", ProjectId = 1, AssignedUserId = null, Status = TaskStatusEnum.Created };
        var taskRepo = new Mock<ITaskRepository>();
        taskRepo.Setup(r => r.QueryForUser("u")).Returns(new List<AppTask> { task }.AsQueryable().BuildMock());
        taskRepo.Setup(r => r.UpdateAsync(task, It.IsAny<bool>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var service = CreateService(taskRepo, new Mock<IProjectRepository>(), new Mock<ITagRepository>(), new Mock<IMediaRepository>(),
            CreateActivityService(new Mock<IProjectActivityLogRepository>()), mapper);

        var dto = new UpdateTaskRequest { Title = "New" };
        await service.UpdateAsync(1, dto, "u");

        Assert.Equal("New", task.Title);
        taskRepo.Verify(r => r.UpdateAsync(task, It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_PercentageCompleteWithoutCompletedStatus_ThrowsRuleViolation()
    {
        var mapper = CreateMapper();
        var task = new AppTask { Id = 1, Title = "Old", Description = "D", ProjectId = 1, Status = TaskStatusEnum.Created };
        var taskRepo = new Mock<ITaskRepository>();
        taskRepo.Setup(r => r.QueryForUser("u")).Returns(new List<AppTask> { task }.AsQueryable().BuildMock());
        var service = CreateService(taskRepo, new Mock<IProjectRepository>(), new Mock<ITagRepository>(), new Mock<IMediaRepository>(),
            CreateActivityService(new Mock<IProjectActivityLogRepository>()), mapper);

        var dto = new UpdateTaskRequest { CompletedPercentage = 100 };

        await Assert.ThrowsAsync<RuleViolationException>(() => service.UpdateAsync(1, dto, "u"));
    }

    [Fact]
    public async Task DeleteAsync_DeletesTask()
    {
        var mapper = CreateMapper();
        var taskRepo = new Mock<ITaskRepository>();
        var task = new AppTask { Id = 1, AuthorId = "u", ProjectId = 1, Title = "T", Description = "D" };
        taskRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        taskRepo.Setup(r => r.DeleteAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var service = CreateService(taskRepo, new Mock<IProjectRepository>(), new Mock<ITagRepository>(), new Mock<IMediaRepository>(),
            CreateActivityService(new Mock<IProjectActivityLogRepository>()), mapper);

        await service.DeleteAsync(1, "u");

        taskRepo.Verify(r => r.DeleteAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddTagAsync_AddsTag()
    {
        var mapper = CreateMapper();
        var tag = new TaskTag { Id = 2, Name = "tag", Color = "#000" };
        var task = new AppTask { Id = 1, ProjectId = 1, Title = "T", Description = "D", Tags = new List<TaskTag>() };
        var taskRepo = new Mock<ITaskRepository>();
        taskRepo.Setup(r => r.QueryForUser("u")).Returns(new List<AppTask> { task }.AsQueryable().BuildMock());
        taskRepo.Setup(r => r.UpdateAsync(task, It.IsAny<bool>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var tagRepo = new Mock<ITagRepository>();
        tagRepo.Setup(t => t.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(tag);
        var service = CreateService(taskRepo, new Mock<IProjectRepository>(), tagRepo, new Mock<IMediaRepository>(),
            CreateActivityService(new Mock<IProjectActivityLogRepository>()), mapper);

        await service.AddTagAsync(1, 2, "u");

        Assert.Contains(tag, task.Tags);
    }

    [Fact]
    public async Task RemoveTagAsync_RemovesTag()
    {
        var mapper = CreateMapper();
        var tag = new TaskTag { Id = 2, Name = "tag", Color = "#000" };
        var task = new AppTask { Id = 1, ProjectId = 1, Title = "T", Description = "D", Tags = new List<TaskTag> { tag } };
        var taskRepo = new Mock<ITaskRepository>();
        taskRepo.Setup(r => r.QueryForUser("u")).Returns(new List<AppTask> { task }.AsQueryable().BuildMock());
        taskRepo.Setup(r => r.UpdateAsync(task, It.IsAny<bool>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var service = CreateService(taskRepo, new Mock<IProjectRepository>(), new Mock<ITagRepository>(), new Mock<IMediaRepository>(),
            CreateActivityService(new Mock<IProjectActivityLogRepository>()), mapper);

        await service.RemoveTagAsync(1, 2, "u");

        Assert.Empty(task.Tags);
    }

    [Fact]
    public async Task GetSubTasksAsync_ReturnsSubtasks()
    {
        var mapper = CreateMapper();
        var child = new AppTask { Id = 2, ParentTaskId = 1, Title = "S", Description = "D", ProjectId = 1, Project = new Project() { Id = 1, Name = "P", OwnerId = "u" } };
        var taskRepo = new Mock<ITaskRepository>();
        taskRepo.Setup(r => r.QueryForUser("u")).Returns(new List<AppTask> { child }.AsQueryable().BuildMock());
        var service = CreateService(taskRepo, new Mock<IProjectRepository>(), new Mock<ITagRepository>(), new Mock<IMediaRepository>(),
            CreateActivityService(new Mock<IProjectActivityLogRepository>()), mapper);

        var result = await service.GetSubTasksAsync(1, "u");

        Assert.Single(result);
        Assert.Equal(2, result.First().Id);
    }

    [Fact]
    public async Task DetachSubTaskAsync_DetachesSubtask()
    {
        var mapper = CreateMapper();
        var sub = new AppTask { Id = 2, ParentTaskId = 1, Title = "S", Description = "D", ProjectId = 1 };
        var taskRepo = new Mock<ITaskRepository>();
        taskRepo.Setup(r => r.QueryForUser("u")).Returns(new List<AppTask> { sub }.AsQueryable().BuildMock());
        taskRepo.Setup(r => r.UpdateAsync(sub, It.IsAny<bool>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var service = CreateService(taskRepo, new Mock<IProjectRepository>(), new Mock<ITagRepository>(), new Mock<IMediaRepository>(),
            CreateActivityService(new Mock<IProjectActivityLogRepository>()), mapper);

        await service.DetachSubTaskAsync(1, 2, "u");

        Assert.Null(sub.ParentTaskId);
    }

    [Fact]
    public async Task AttachMediaAsync_AttachesMedia()
    {
        var mapper = CreateMapper();
        var media = new Media { Id = 5, UploadedById = "u", FileName = "f.jpg", CollectionName = "c", Name = "n", Disk = "d", Uuid = Guid.NewGuid(), AccessScope = AccessScope.Private };
        var task = new AppTask { Id = 1, ProjectId = 1, Title = "T", Description = "D" };
        var taskRepo = new Mock<ITaskRepository>();
        taskRepo.Setup(r => r.QueryForUser("u")).Returns(new List<AppTask> { task }.AsQueryable().BuildMock());
        taskRepo.Setup(r => r.UpdateAsync(It.IsAny<AppTask>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        taskRepo.Setup(r => r.Query()).Returns(new List<AppTask> { task }.AsQueryable().BuildMock());
        var mediaRepo = new Mock<IMediaRepository>();
        mediaRepo.Setup(m => m.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(media);
        mediaRepo.Setup(m => m.UpdateAsync(media, It.IsAny<bool>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var service = CreateService(taskRepo, new Mock<IProjectRepository>(), new Mock<ITagRepository>(), mediaRepo,
            CreateActivityService(new Mock<IProjectActivityLogRepository>()), mapper);

        await service.AttachMediaAsync(1, 5, "u");

        Assert.Equal("1", media.ModelId);
        Assert.Equal(nameof(AppTask), media.ModelType);
    }

    [Fact]
    public async Task GetAttachmentsAsync_ReturnsAttachments()
    {
        var mapper = CreateMapper();
        var media = new Media { Id = 5, UploadedById = "u", FileName = "f.jpg", CollectionName = "c", Name = "n", Disk = "d", Uuid = Guid.NewGuid(), ModelId = "1", ModelType = nameof(AppTask), AccessScope = AccessScope.Private };
        var taskRepo = new Mock<ITaskRepository>();
        taskRepo.Setup(r => r.QueryForUser("u")).Returns(new List<AppTask> { new AppTask { Id = 1, ProjectId = 1, Title = "T", Description = "D" } }.AsQueryable().BuildMock());
        var mediaRepo = new Mock<IMediaRepository>();
        mediaRepo.Setup(m => m.Query()).Returns(new List<Media> { media }.AsQueryable().BuildMock());
        var service = CreateService(taskRepo, new Mock<IProjectRepository>(), new Mock<ITagRepository>(), mediaRepo,
            CreateActivityService(new Mock<IProjectActivityLogRepository>()), mapper);

        var result = await service.GetAttachmentsAsync(1, "u");

        Assert.Single(result);
        Assert.Equal(5, result.First().Id);
    }
}

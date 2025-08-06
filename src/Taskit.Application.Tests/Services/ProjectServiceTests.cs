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
using Taskit.Application.Common.Exceptions;
using Ardalis.GuardClauses;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Taskit.Application.Tests.Services;

public class ProjectServiceTests
{
    private static IMapper CreateMapper()
    {
        var mockLogger = new Mock<ILogger<ProjectService>>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        mockLoggerFactory
            .Setup(f => f.CreateLogger(It.IsAny<string>()))
            .Returns(mockLogger.Object);

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Project, ProjectDto>();
            cfg.CreateMap<CreateProjectRequest, Project>();
            cfg.CreateMap<UpdateProjectRequest, Project>()
                .ForAllMembers(o => o.Condition((src, dest, member) => member != null));
            cfg.CreateMap<AppUser, DTOs.UserDto>();
        }, mockLoggerFactory.Object);
        return config.CreateMapper();
    }

    private static ProjectActivityLogService CreateActivityService(Mock<IProjectActivityLogRepository> repo)
    {
        var mapper = CreateMapper();
        var publisher = new Mock<IPublishEndpoint>();
        repo.Setup(r => r.AddAsync(It.IsAny<ProjectActivityLog>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);
        repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        return new ProjectActivityLogService(repo.Object, mapper, publisher.Object);
    }

    private static ProjectService CreateService(Mock<IProjectRepository> repo, ProjectActivityLogService activity, IMapper mapper)
    {
        return new ProjectService(repo.Object, mapper, activity);
    }

    [Fact]
    public async Task GetAllForUserAsync_ReturnsAccessibleProjects()
    {
        var mapper = CreateMapper();
        var projects = new List<Project>
        {
            new() { Id = 1, Name = "P1", OwnerId = "u" },
            new() { Id = 2, Name = "P2", OwnerId = "o", Members = new List<ProjectMember>
            {
                new ProjectMember { Id = 1, ProjectId = 2, UserId = "u", Role = ProjectRole.Member }
            }}
        };
        var repo = new Mock<IProjectRepository>();
        repo.Setup(r => r.Query()).Returns(projects.AsQueryable().BuildMock());
        var service = CreateService(repo, CreateActivityService(new Mock<IProjectActivityLogRepository>()), mapper);

        var result = await service.GetAllForUserAsync("u", new GridifyQuery());

        Assert.Equal(2, result.Data.Count());
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsProject()
    {
        var mapper = CreateMapper();
        var project = new Project { Id = 1, Name = "P", OwnerId = "u" };
        var repo = new Mock<IProjectRepository>();
        repo.Setup(r => r.Query()).Returns(new List<Project> { project }.AsQueryable().BuildMockDbSet().Object);
        var service = CreateService(repo, CreateActivityService(new Mock<IProjectActivityLogRepository>()), mapper);

        var result = await service.GetByIdAsync(1, "u");

        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ThrowsNotFound()
    {
        var mapper = CreateMapper();
        var repo = new Mock<IProjectRepository>();
        repo.Setup(r => r.Query()).Returns(new List<Project>().AsQueryable().BuildMock());
        var service = CreateService(repo, CreateActivityService(new Mock<IProjectActivityLogRepository>()), mapper);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(1, "u"));
    }

    [Fact]
    public async Task CreateAsync_SavesProject()
    {
        var mapper = CreateMapper();
        var repo = new Mock<IProjectRepository>();
        repo.Setup(r => r.AddAsync(It.IsAny<Project>(), It.IsAny<bool>())).Returns(Task.CompletedTask);
        var service = CreateService(repo, CreateActivityService(new Mock<IProjectActivityLogRepository>()), mapper);

        var dto = new CreateProjectRequest { Name = "N", Description = "D" };
        var result = await service.CreateAsync(dto, "u");

        repo.Verify(r => r.AddAsync(It.Is<Project>(p => p.OwnerId == "u" && p.Name == "N"), It.IsAny<bool>()), Times.Once);
        Assert.Equal("N", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesProject()
    {
        var mapper = CreateMapper();
        var project = new Project { Id = 1, Name = "Old", OwnerId = "u" };
        var repo = new Mock<IProjectRepository>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(project);
        repo.Setup(r => r.UpdateAsync(project, It.IsAny<bool>())).Returns(Task.CompletedTask);
        repo.Setup(r => r.Query()).Returns(new List<Project> { project }.AsQueryable().BuildMockDbSet().Object);
        var service = CreateService(repo, CreateActivityService(new Mock<IProjectActivityLogRepository>()), mapper);

        var dto = new UpdateProjectRequest { Name = "New" };
        await service.UpdateAsync(1, dto, "u");

        Assert.Equal("New", project.Name);
        repo.Verify(r => r.UpdateAsync(project, It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NotOwner_ThrowsForbidden()
    {
        var mapper = CreateMapper();
        var project = new Project { Id = 1, Name = "P", OwnerId = "owner" };
        var repo = new Mock<IProjectRepository>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(project);
        var service = CreateService(repo, CreateActivityService(new Mock<IProjectActivityLogRepository>()), mapper);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() => service.UpdateAsync(1, new UpdateProjectRequest { Name = "N" }, "u"));
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ThrowsNotFound()
    {
        var mapper = CreateMapper();
        var repo = new Mock<IProjectRepository>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Project?)null);
        var service = CreateService(repo, CreateActivityService(new Mock<IProjectActivityLogRepository>()), mapper);

        await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateAsync(1, new UpdateProjectRequest { Name = "N" }, "u"));
    }

    [Fact]
    public async Task UpdateAsync_DuplicateName_ThrowsRuleViolation()
    {
        var mapper = CreateMapper();
        var project = new Project { Id = 1, Name = "Old", OwnerId = "u" };
        var repo = new Mock<IProjectRepository>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(project);
        repo.Setup(r => r.Query()).Returns(new List<Project>
        {
            project,
            new() { Id = 2, Name = "New", OwnerId = "u" }
        }.AsQueryable().BuildMockDbSet().Object);
        var service = CreateService(repo, CreateActivityService(new Mock<IProjectActivityLogRepository>()), mapper);

        var dto = new UpdateProjectRequest { Name = "New" };

        await Assert.ThrowsAsync<RuleViolationException>(() => service.UpdateAsync(1, dto, "u"));
    }

    [Fact]
    public async Task DeleteAsync_DeletesProject()
    {
        var mapper = CreateMapper();
        var project = new Project { Id = 1, Name = "P", OwnerId = "u" };
        var repo = new Mock<IProjectRepository>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(project);
        repo.Setup(r => r.DeleteAsync(1, It.IsAny<bool>())).Returns(Task.CompletedTask);
        var service = CreateService(repo, CreateActivityService(new Mock<IProjectActivityLogRepository>()), mapper);

        await service.DeleteAsync(1, "u");

        repo.Verify(r => r.DeleteAsync(1, It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NotOwner_ThrowsForbidden()
    {
        var mapper = CreateMapper();
        var project = new Project { Id = 1, Name = "P", OwnerId = "owner" };
        var repo = new Mock<IProjectRepository>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(project);
        var service = CreateService(repo, CreateActivityService(new Mock<IProjectActivityLogRepository>()), mapper);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() => service.DeleteAsync(1, "u"));
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ThrowsNotFound()
    {
        var mapper = CreateMapper();
        var repo = new Mock<IProjectRepository>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Project?)null);
        var service = CreateService(repo, CreateActivityService(new Mock<IProjectActivityLogRepository>()), mapper);

        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(1, "u"));
    }
}

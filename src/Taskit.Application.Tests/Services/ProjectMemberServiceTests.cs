using System.Collections.Generic;
using AutoMapper;
using Gridify;
using MockQueryable.Moq;
using Moq;
using Taskit.Application.DTOs;
using Taskit.Application.Interfaces;
using Taskit.Application.Services;
using Taskit.Application.Common.Exceptions;
using Taskit.Domain.Entities;
using Taskit.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using MassTransit;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Linq;
using Ardalis.GuardClauses;

namespace Taskit.Application.Tests.Services;

public class ProjectMemberServiceTests
{
    private static IMapper CreateMapper()
    {
        var logger = new Mock<ILogger<ProjectMemberService>>();
        var factory = new Mock<ILoggerFactory>();
        factory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<AppUser, UserProfileDto>();
            cfg.CreateMap<ProjectMember, ProjectMemberDto>();
            cfg.CreateMap<AddProjectMemberRequest, ProjectMember>();
            cfg.CreateMap<UpdateProjectMemberRequest, ProjectMember>()
                .ForAllMembers(o => o.Condition((src, dest, member) => member != null));
        }, factory.Object);
        return config.CreateMapper();
    }

    private static Mock<UserManager<AppUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<AppUser>>();
#pragma warning disable CS8625
        return new Mock<UserManager<AppUser>>(store.Object, null, null, null, null, null, null, null, null);
#pragma warning restore CS8625
    }

    private static ProjectActivityLogService CreateActivityService(IMapper mapper)
    {
        var repo = new Mock<IProjectActivityLogRepository>();
        repo.Setup(r => r.AddAsync(It.IsAny<ProjectActivityLog>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var publisher = new Mock<IPublishEndpoint>();
        return new ProjectActivityLogService(repo.Object, mapper, publisher.Object);
    }

    private static ProjectMemberService CreateService(
        Mock<IProjectMemberRepository> members,
        Mock<IProjectRepository> projects,
        Mock<UserManager<AppUser>> users,
        IMapper mapper)
    {
        var activity = CreateActivityService(mapper);
        return new ProjectMemberService(members.Object, projects.Object, users.Object, mapper, activity);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMembers()
    {
        var mapper = CreateMapper();
        var user = new AppUser { Id = "u", UserName = "user" };
        var member = new ProjectMember { Id = 1, ProjectId = 1, UserId = "u", User = user, Role = ProjectRole.Member };
        var project = new Project { Id = 1, Name = "P", OwnerId = "owner", Members = new List<ProjectMember> { member } };
        var memberRepo = new Mock<IProjectMemberRepository>();
        memberRepo.Setup(r => r.QueryForProject(1)).Returns(new List<ProjectMember> { member }.AsQueryable().BuildMock());
        var projectRepo = new Mock<IProjectRepository>();
        projectRepo.Setup(r => r.Query()).Returns(new List<Project> { project }.AsQueryable().BuildMock());
        var userManager = MockUserManager();
        var service = CreateService(memberRepo, projectRepo, userManager, mapper);

        var result = await service.GetAllAsync(1, "owner", new GridifyQuery());

        Assert.Single(result.Data);
        Assert.Equal(1, result.Data.First().Id);
    }

    [Fact]
    public async Task GetAllAsync_UserWithoutAccess_ThrowsForbidden()
    {
        var mapper = CreateMapper();
        var project = new Project { Id = 1, Name = "P", OwnerId = "owner" };
        var memberRepo = new Mock<IProjectMemberRepository>();
        var projectRepo = new Mock<IProjectRepository>();
        projectRepo.Setup(r => r.Query()).Returns(new List<Project> { project }.AsQueryable().BuildMock());
        var userManager = MockUserManager();
        var service = CreateService(memberRepo, projectRepo, userManager, mapper);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() => service.GetAllAsync(1, "u", new GridifyQuery()));
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsMember()
    {
        var mapper = CreateMapper();
        var user = new AppUser { Id = "u", UserName = "user" };
        var member = new ProjectMember { Id = 1, ProjectId = 1, UserId = "u", User = user, Role = ProjectRole.Member };
        var project = new Project { Id = 1, Name = "P", OwnerId = "owner", Members = new List<ProjectMember> { member } };
        var memberRepo = new Mock<IProjectMemberRepository>();
        memberRepo.Setup(r => r.QueryForProject(1)).Returns(new List<ProjectMember> { member }.AsQueryable().BuildMock());
        var projectRepo = new Mock<IProjectRepository>();
        projectRepo.Setup(r => r.Query()).Returns(new List<Project> { project }.AsQueryable().BuildMock());
        var userManager = MockUserManager();
        var service = CreateService(memberRepo, projectRepo, userManager, mapper);

        var result = await service.GetByIdAsync(1, 1, "owner");

        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_MemberNotFound_ThrowsNotFound()
    {
        var mapper = CreateMapper();
        var project = new Project { Id = 1, Name = "P", OwnerId = "owner" };
        var memberRepo = new Mock<IProjectMemberRepository>();
        memberRepo.Setup(r => r.QueryForProject(1)).Returns(new List<ProjectMember>().AsQueryable().BuildMock());
        var projectRepo = new Mock<IProjectRepository>();
        projectRepo.Setup(r => r.Query()).Returns(new List<Project> { project }.AsQueryable().BuildMock());
        var userManager = MockUserManager();
        var service = CreateService(memberRepo, projectRepo, userManager, mapper);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(1, 1, "owner"));
    }

    [Fact]
    public async Task AddAsync_AddsMember()
    {
        var mapper = CreateMapper();
        var project = new Project { Id = 1, Name = "P", OwnerId = "owner", Members = new List<ProjectMember>() };
        var memberRepo = new Mock<IProjectMemberRepository>();
        memberRepo.Setup(r => r.AddAsync(It.IsAny<ProjectMember>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();
        var projectRepo = new Mock<IProjectRepository>();
        projectRepo.Setup(r => r.Query()).Returns(new List<Project> { project }.AsQueryable().BuildMock());
        var userManager = MockUserManager();
        var newUser = new AppUser { Id = "u", UserName = "user" };
        userManager.Setup(u => u.FindByIdAsync("u")).ReturnsAsync(newUser);
        var service = CreateService(memberRepo, projectRepo, userManager, mapper);

        var dto = new AddProjectMemberRequest { UserId = "u", Role = ProjectRole.Member };
        var result = await service.AddAsync(1, dto, "owner");

        memberRepo.Verify(r => r.AddAsync(It.Is<ProjectMember>(m => m.UserId == "u" && m.ProjectId == 1), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal("u", result.User.Id);
    }

    [Fact]
    public async Task AddAsync_UserAlreadyMember_ThrowsRuleViolation()
    {
        var mapper = CreateMapper();
        var member = new ProjectMember { Id = 1, ProjectId = 1, UserId = "u", Role = ProjectRole.Member };
        var project = new Project { Id = 1, Name = "P", OwnerId = "owner", Members = new List<ProjectMember> { member } };
        var memberRepo = new Mock<IProjectMemberRepository>();
        var projectRepo = new Mock<IProjectRepository>();
        projectRepo.Setup(r => r.Query()).Returns(new List<Project> { project }.AsQueryable().BuildMock());
        var userManager = MockUserManager();
        var service = CreateService(memberRepo, projectRepo, userManager, mapper);

        var dto = new AddProjectMemberRequest { UserId = "u", Role = ProjectRole.Member };
        await Assert.ThrowsAsync<RuleViolationException>(() => service.AddAsync(1, dto, "owner"));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesMember()
    {
        var mapper = CreateMapper();
        var project = new Project { Id = 1, Name = "P", OwnerId = "owner" };
        var member = new ProjectMember { Id = 1, ProjectId = 1, Project = project, UserId = "u", Role = ProjectRole.Member };
        project.Members = new List<ProjectMember> { member };
        var memberRepo = new Mock<IProjectMemberRepository>();
        memberRepo.Setup(r => r.Query()).Returns(new List<ProjectMember> { member }.AsQueryable().BuildMock());
        memberRepo.Setup(r => r.UpdateAsync(member, It.IsAny<bool>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();
        var projectRepo = new Mock<IProjectRepository>();
        var userManager = MockUserManager();
        var service = CreateService(memberRepo, projectRepo, userManager, mapper);

        var dto = new UpdateProjectMemberRequest { Role = ProjectRole.Admin };
        await service.UpdateAsync(1, 1, dto, "owner");

        Assert.Equal(ProjectRole.Admin, member.Role);
        memberRepo.Verify(r => r.UpdateAsync(member, It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NotManager_ThrowsForbidden()
    {
        var mapper = CreateMapper();
        var project = new Project { Id = 1, Name = "P", OwnerId = "owner" };
        var member = new ProjectMember { Id = 1, ProjectId = 1, Project = project, UserId = "u", Role = ProjectRole.Member };
        project.Members = new List<ProjectMember> { member, new ProjectMember { Id = 2, ProjectId = 1, UserId = "other", Role = ProjectRole.Member } };
        var memberRepo = new Mock<IProjectMemberRepository>();
        memberRepo.Setup(r => r.Query()).Returns(new List<ProjectMember> { member }.AsQueryable().BuildMock());
        var projectRepo = new Mock<IProjectRepository>();
        var userManager = MockUserManager();
        var service = CreateService(memberRepo, projectRepo, userManager, mapper);

        var dto = new UpdateProjectMemberRequest { Role = ProjectRole.Admin };
        await Assert.ThrowsAsync<ForbiddenAccessException>(() => service.UpdateAsync(1, 1, dto, "other"));
    }

    [Fact]
    public async Task UpdateAsync_SetOwnerRole_ThrowsRuleViolation()
    {
        var mapper = CreateMapper();
        var project = new Project { Id = 1, Name = "P", OwnerId = "owner" };
        var member = new ProjectMember { Id = 1, ProjectId = 1, Project = project, UserId = "u", Role = ProjectRole.Member };
        project.Members = new List<ProjectMember> { member };
        var memberRepo = new Mock<IProjectMemberRepository>();
        memberRepo.Setup(r => r.Query()).Returns(new List<ProjectMember> { member }.AsQueryable().BuildMock());
        var projectRepo = new Mock<IProjectRepository>();
        var userManager = MockUserManager();
        var service = CreateService(memberRepo, projectRepo, userManager, mapper);

        var dto = new UpdateProjectMemberRequest { Role = ProjectRole.Owner };

        await Assert.ThrowsAsync<RuleViolationException>(() => service.UpdateAsync(1, 1, dto, "owner"));
    }

    [Fact]
    public async Task DeleteAsync_DeletesMember()
    {
        var mapper = CreateMapper();
        var project = new Project { Id = 1, Name = "P", OwnerId = "owner" };
        var member = new ProjectMember { Id = 1, ProjectId = 1, Project = project, UserId = "u", Role = ProjectRole.Member };
        project.Members = new List<ProjectMember> { member };
        var memberRepo = new Mock<IProjectMemberRepository>();
        memberRepo.Setup(r => r.Query()).Returns(new List<ProjectMember> { member }.AsQueryable().BuildMock());
        memberRepo.Setup(r => r.DeleteAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();
        var projectRepo = new Mock<IProjectRepository>();
        var userManager = MockUserManager();
        var service = CreateService(memberRepo, projectRepo, userManager, mapper);

        await service.DeleteAsync(1, 1, "owner");

        memberRepo.Verify(r => r.DeleteAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NotManager_ThrowsForbidden()
    {
        var mapper = CreateMapper();
        var project = new Project { Id = 1, Name = "P", OwnerId = "owner" };
        var member = new ProjectMember { Id = 1, ProjectId = 1, Project = project, UserId = "u", Role = ProjectRole.Member };
        var requester = new ProjectMember { Id = 2, ProjectId = 1, Project = project, UserId = "other", Role = ProjectRole.Member };
        project.Members = new List<ProjectMember> { member, requester };
        var memberRepo = new Mock<IProjectMemberRepository>();
        memberRepo.Setup(r => r.Query()).Returns(new List<ProjectMember> { member }.AsQueryable().BuildMock());
        var projectRepo = new Mock<IProjectRepository>();
        var userManager = MockUserManager();
        var service = CreateService(memberRepo, projectRepo, userManager, mapper);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() => service.DeleteAsync(1, 1, "other"));
    }
}

using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Taskit.Application.Services;
using Taskit.Application.DTOs;
using Taskit.Application.Common.Settings;
using Taskit.Application.Common.Exceptions;
using Taskit.Domain.Entities;
using Taskit.Application.Interfaces;

namespace Taskit.Application.Tests.Services;

public class AuthServiceTests
{
    private static Mock<UserManager<AppUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<AppUser>>();
#pragma warning disable CS8625
        return new Mock<UserManager<AppUser>>(store.Object, null, null, null, null, null, null, null, null);
#pragma warning restore CS8625
    }

    private static Mock<SignInManager<AppUser>> MockSignInManager(Mock<UserManager<AppUser>> userManager)
    {
#pragma warning disable CS8625
        return new Mock<SignInManager<AppUser>>(userManager.Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<AppUser>>().Object,
            null, null, null, null);
#pragma warning restore CS8625
    }

    private static IMapper CreateMapper()
    {
        var factory = new Mock<ILoggerFactory>();
        factory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<AppUser, UserDto>()
                .ForMember(d => d.AvatarUrl, o => o.MapFrom(s => s.Avatar != null ? $"http://localhost:5152/api/media/{s.Avatar.Id}" : null));
            cfg.CreateMap<AppUser, UserProfileDto>()
                .ForMember(d => d.AvatarUrl, o => o.MapFrom(s => s.Avatar != null ? $"http://localhost:5152/api/media/{s.Avatar.Id}" : null));
        }, factory.Object);
        return config.CreateMapper();
    }

    private static AuthService CreateService(Mock<UserManager<AppUser>> userManager,
        Mock<SignInManager<AppUser>> signInManager,
        Mock<IRefreshTokenRepository> repo)
    {
        var settings = Options.Create(new JwtSettings
        {
            Key = "01234567890123456789012345678901",
            Issuer = "issuer",
            Audience = "audience",
            AccessTokenExpirationMinutes = 1,
            RefreshTokenExpirationDays = 7
        });
        var mapper = CreateMapper();
        return new AuthService(userManager.Object, signInManager.Object, settings, repo.Object, mapper);
    }

    private static string ComputeSha256(string raw)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes);
    }

    [Fact]
    public async Task RegisterAsync_CreatesUser()
    {
        var userManager = MockUserManager();
        userManager.Setup(u => u.CreateAsync(It.IsAny<AppUser>(), "pass"))
            .ReturnsAsync(IdentityResult.Success);
        var signIn = MockSignInManager(userManager);
        var repo = new Mock<IRefreshTokenRepository>();
        var service = CreateService(userManager, signIn, repo);

        var dto = new RegisterRequest
        {
            Username = "user",
            Email = "user@example.com",
            FullName = "User",
            Password = "pass"
        };

        await service.RegisterAsync(dto);

        userManager.Verify(u => u.CreateAsync(
            It.Is<AppUser>(au => au.UserName == dto.Username &&
                                 au.Email == dto.Email &&
                                 au.FullName == dto.FullName),
            dto.Password), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WhenCreationFails_ThrowsValidationException()
    {
        var userManager = MockUserManager();
        userManager.Setup(u => u.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "E", Description = "err" }));
        var signIn = MockSignInManager(userManager);
        var repo = new Mock<IRefreshTokenRepository>();
        var service = CreateService(userManager, signIn, repo);

        var dto = new RegisterRequest { Username = "user", Email = "user@example.com", FullName = "User", Password = "pass" };

        await Assert.ThrowsAsync<ValidationException>(() => service.RegisterAsync(dto));
    }

    [Fact]
    public async Task LoginAsync_ReturnsTokens()
    {
        var user = new AppUser { Id = "1", UserName = "user", Email = "u@example.com", FullName = "User" };
        var userManager = MockUserManager();
        userManager.Setup(u => u.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        var signIn = MockSignInManager(userManager);
        signIn.Setup(s => s.CheckPasswordSignInAsync(user, "pass", false))
            .ReturnsAsync(SignInResult.Success);
        var repo = new Mock<IRefreshTokenRepository>();
        repo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<bool>())).Returns(Task.CompletedTask);
        var service = CreateService(userManager, signIn, repo);

        var dto = new LoginRequest { Email = user.Email!, Password = "pass" };

        var response = await service.LoginAsync(dto);

        Assert.False(string.IsNullOrWhiteSpace(response.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(response.RefreshToken));
        Assert.NotNull(response.User);
        Assert.Equal(user.Email, response.User?.Email);
        repo.Verify(r => r.AddAsync(It.Is<RefreshToken>(t => t.UserId == user.Id), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ThrowsUnauthorized()
    {
        var userManager = MockUserManager();
        userManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((AppUser?)null);
        var signIn = MockSignInManager(userManager);
        var repo = new Mock<IRefreshTokenRepository>();
        var service = CreateService(userManager, signIn, repo);

        var dto = new LoginRequest { Email = "u@example.com", Password = "pass" };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(dto));
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsUnauthorized()
    {
        var user = new AppUser { Id = "1", UserName = "user", Email = "u@example.com" };
        var userManager = MockUserManager();
        userManager.Setup(u => u.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        var signIn = MockSignInManager(userManager);
        signIn.Setup(s => s.CheckPasswordSignInAsync(user, "wrong", false))
            .ReturnsAsync(SignInResult.Failed);
        var repo = new Mock<IRefreshTokenRepository>();
        var service = CreateService(userManager, signIn, repo);

        var dto = new LoginRequest { Email = user.Email!, Password = "wrong" };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(dto));
    }

    [Fact]
    public async Task LogoutAsync_CallsSignOut()
    {
        var userManager = MockUserManager();
        var signIn = MockSignInManager(userManager);
        signIn.Setup(s => s.SignOutAsync()).Returns(Task.CompletedTask).Verifiable();
        var repo = new Mock<IRefreshTokenRepository>();
        var service = CreateService(userManager, signIn, repo);

        await service.LogoutAsync();

        signIn.Verify(s => s.SignOutAsync(), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_WhenTokenIsNull_ThrowsUnauthorized()
    {
        var userManager = MockUserManager();
        var signIn = MockSignInManager(userManager);
        var repo = new Mock<IRefreshTokenRepository>();
        var service = CreateService(userManager, signIn, repo);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.RefreshAsync(null!));
    }

    [Fact]
    public async Task RefreshAsync_TokenNotFound_ThrowsUnauthorized()
    {
        var userManager = MockUserManager();
        var signIn = MockSignInManager(userManager);
        var repo = new Mock<IRefreshTokenRepository>();
        repo.Setup(r => r.GetByTokenAsync(It.IsAny<string>())).ReturnsAsync((RefreshToken?)null);
        var service = CreateService(userManager, signIn, repo);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.RefreshAsync("token"));
    }

    [Fact]
    public async Task RefreshAsync_RevokedToken_ThrowsUnauthorized()
    {
        var userManager = MockUserManager();
        var signIn = MockSignInManager(userManager);
        var repo = new Mock<IRefreshTokenRepository>();
        var user = new AppUser { Id = "1", UserName = "user", Email = "u@example.com" };
        var hash = ComputeSha256("token");
        var refresh = new RefreshToken { TokenHash = hash, UserId = user.Id, User = user, ExpiresAt = DateTime.UtcNow.AddMinutes(10), RevokedAt = DateTime.UtcNow };
        repo.Setup(r => r.GetByTokenAsync(hash)).ReturnsAsync(refresh);
        var service = CreateService(userManager, signIn, repo);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.RefreshAsync("token"));
    }

    [Fact]
    public async Task RefreshAsync_ExpiredToken_ThrowsUnauthorized()
    {
        var userManager = MockUserManager();
        var signIn = MockSignInManager(userManager);
        var repo = new Mock<IRefreshTokenRepository>();
        var user = new AppUser { Id = "1", UserName = "user", Email = "u@example.com" };
        var hash = ComputeSha256("token");
        var refresh = new RefreshToken { TokenHash = hash, UserId = user.Id, User = user, ExpiresAt = DateTime.UtcNow.AddMinutes(-1) };
        repo.Setup(r => r.GetByTokenAsync(hash)).ReturnsAsync(refresh);
        var service = CreateService(userManager, signIn, repo);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.RefreshAsync("token"));
    }

    [Fact]
    public async Task RefreshAsync_ReturnsNewTokens()
    {
        var userManager = MockUserManager();
        var signIn = MockSignInManager(userManager);
        var repo = new Mock<IRefreshTokenRepository>();
        var user = new AppUser { Id = "1", UserName = "user", Email = "u@example.com" };
        var hash = ComputeSha256("token");
        var refresh = new RefreshToken { TokenHash = hash, UserId = user.Id, User = user, ExpiresAt = DateTime.UtcNow.AddMinutes(10) };
        repo.Setup(r => r.GetByTokenAsync(hash)).ReturnsAsync(refresh);
        repo.Setup(r => r.UpdateAsync(refresh, It.IsAny<bool>())).Returns(Task.CompletedTask);
        repo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<bool>())).Returns(Task.CompletedTask);
        var service = CreateService(userManager, signIn, repo);

        var response = await service.RefreshAsync("token");

        Assert.False(string.IsNullOrWhiteSpace(response.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(response.RefreshToken));
        repo.Verify(r => r.UpdateAsync(It.Is<RefreshToken>(rt => rt == refresh && rt.RevokedAt != null), It.IsAny<bool>()), Times.Once);
        repo.Verify(r => r.AddAsync(It.Is<RefreshToken>(rt => rt.UserId == user.Id), It.IsAny<bool>()), Times.Once);
    }
}
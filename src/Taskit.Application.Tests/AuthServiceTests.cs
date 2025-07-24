using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using Taskit.Application.Common.Exceptions;
using Taskit.Application.Common.Settings;
using Taskit.Application.DTOs;
using Taskit.Application.Interfaces;
using Taskit.Application.Services;
using Taskit.Domain.Entities;
using Xunit;

namespace Taskit.Application.Tests;

public class AuthServiceTests
{
    private static Mock<UserManager<AppUser>> GetUserManagerMock()
    {
        var store = new Mock<IUserStore<AppUser>>();
        return new Mock<UserManager<AppUser>>(store.Object, null, null, null, null, null, null, null, null);
    }

    private static Mock<SignInManager<AppUser>> GetSignInManagerMock(UserManager<AppUser> userManager)
    {
        return new Mock<SignInManager<AppUser>>(userManager,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<AppUser>>().Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<ILogger<SignInManager<AppUser>>>().Object,
            new Mock<IAuthenticationSchemeProvider>().Object,
            new Mock<IUserConfirmation<AppUser>>().Object);
    }

    private static AuthService CreateService(Mock<UserManager<AppUser>> userManager,
        Mock<SignInManager<AppUser>> signInManager,
        Mock<IRefreshTokenRepository> refreshRepo,
        JwtSettings? settings = null)
    {
        settings ??= new JwtSettings
        {
            Key = "0123456789ABCDEF0123456789ABCDEF",
            Issuer = "test",
            Audience = "test",
            AccessTokenExpirationMinutes = 60,
            RefreshTokenExpirationDays = 7
        };

        return new AuthService(userManager.Object, signInManager.Object,
            Options.Create(settings), refreshRepo.Object);
    }

    [Fact]
    public async Task RegisterAsync_CreatesUser()
    {
        var userManager = GetUserManagerMock();
        var signInManager = GetSignInManagerMock(userManager.Object);
        var refreshRepo = new Mock<IRefreshTokenRepository>();
        userManager.Setup(u => u.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        var service = CreateService(userManager, signInManager, refreshRepo);
        var dto = new RegisterRequest
        {
            Username = "test",
            Email = "test@example.com",
            Password = "Password1!",
            FullName = "Test User"
        };

        await service.RegisterAsync(dto);

        userManager.Verify(m => m.CreateAsync(
            It.Is<AppUser>(u => u.UserName == dto.Username && u.Email == dto.Email && u.FullName == dto.FullName),
            dto.Password), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WhenCreateFails_ThrowsValidationException()
    {
        var userManager = GetUserManagerMock();
        var signInManager = GetSignInManagerMock(userManager.Object);
        var refreshRepo = new Mock<IRefreshTokenRepository>();
        userManager.Setup(u => u.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "code", Description = "desc" }));
        var service = CreateService(userManager, signInManager, refreshRepo);
        var dto = new RegisterRequest
        {
            Username = "test",
            Email = "test@example.com",
            Password = "Password1!",
            FullName = "Test User"
        };

        await Assert.ThrowsAsync<ValidationException>(() => service.RegisterAsync(dto));
    }

    [Fact]
    public async Task LoginAsync_ReturnsTokensAndUser()
    {
        var userManager = GetUserManagerMock();
        var signInManager = GetSignInManagerMock(userManager.Object);
        var refreshRepo = new Mock<IRefreshTokenRepository>();

        var user = new AppUser { Id = "1", UserName = "test", Email = "test@example.com", FullName = "Test" };
        userManager.Setup(u => u.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        signInManager.Setup(s => s.CheckPasswordSignInAsync(user, It.IsAny<string>(), false))
            .ReturnsAsync(SignInResult.Success);

        RefreshToken? stored = null;
        refreshRepo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>(), true))
            .Callback<RefreshToken, bool>((rt, _) => stored = rt)
            .Returns(Task.CompletedTask);

        var service = CreateService(userManager, signInManager, refreshRepo);
        var dto = new LoginRequest { Email = user.Email!, Password = "Password1!" };
        var result = await service.LoginAsync(dto, "agent", IPAddress.Parse("127.0.0.1"));

        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);
        Assert.Equal(user.UserName, result.User.UserName);
        Assert.Equal(user.Email, result.User.Email);
        Assert.Equal(user.FullName, result.User.FullName);

        Assert.NotNull(stored);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(result.RefreshToken!));
        Assert.Equal(Convert.ToHexString(hash), stored!.TokenHash);
        Assert.Equal(user.Id, stored.UserId);
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ThrowsUnauthorized()
    {
        var userManager = GetUserManagerMock();
        var signInManager = GetSignInManagerMock(userManager.Object);
        var refreshRepo = new Mock<IRefreshTokenRepository>();
        userManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((AppUser?)null);
        var service = CreateService(userManager, signInManager, refreshRepo);
        var dto = new LoginRequest { Email = "no@user.com", Password = "Password1!" };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(dto));
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsUnauthorized()
    {
        var userManager = GetUserManagerMock();
        var signInManager = GetSignInManagerMock(userManager.Object);
        var refreshRepo = new Mock<IRefreshTokenRepository>();
        var user = new AppUser { Id = "1", UserName = "test", Email = "test@example.com" };
        userManager.Setup(u => u.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        signInManager.Setup(s => s.CheckPasswordSignInAsync(user, It.IsAny<string>(), false))
            .ReturnsAsync(SignInResult.Failed);
        var service = CreateService(userManager, signInManager, refreshRepo);
        var dto = new LoginRequest { Email = user.Email!, Password = "wrong" };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(dto));
    }

    [Fact]
    public async Task LogoutAsync_CallsSignOut()
    {
        var userManager = GetUserManagerMock();
        var signInManager = GetSignInManagerMock(userManager.Object);
        var refreshRepo = new Mock<IRefreshTokenRepository>();
        signInManager.Setup(s => s.SignOutAsync()).Returns(Task.CompletedTask).Verifiable();
        var service = CreateService(userManager, signInManager, refreshRepo);

        await service.LogoutAsync();

        signInManager.Verify(s => s.SignOutAsync(), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_ValidToken_ReturnsNewTokens()
    {
        var userManager = GetUserManagerMock();
        var signInManager = GetSignInManagerMock(userManager.Object);
        var refreshRepo = new Mock<IRefreshTokenRepository>();
        var user = new AppUser { Id = "1", UserName = "test", Email = "test@example.com" };
        var existingToken = "token";
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(existingToken)));
        var stored = new RefreshToken
        {
            TokenHash = tokenHash,
            UserId = user.Id,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };
        refreshRepo.Setup(r => r.GetByTokenAsync(tokenHash)).ReturnsAsync(stored);
        refreshRepo.Setup(r => r.UpdateAsync(stored, true)).Returns(Task.CompletedTask).Verifiable();
        RefreshToken? newToken = null;
        refreshRepo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>(), true))
            .Callback<RefreshToken, bool>((rt, _) => newToken = rt)
            .Returns(Task.CompletedTask);
        var service = CreateService(userManager, signInManager, refreshRepo);

        var result = await service.RefreshAsync(existingToken, "agent", IPAddress.Parse("127.0.0.1"));

        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);
        Assert.NotNull(stored.RevokedAt);
        refreshRepo.Verify(r => r.UpdateAsync(stored, true), Times.Once);
        Assert.NotNull(newToken);
    }

    [Fact]
    public async Task RefreshAsync_InvalidToken_ThrowsUnauthorized()
    {
        var userManager = GetUserManagerMock();
        var signInManager = GetSignInManagerMock(userManager.Object);
        var refreshRepo = new Mock<IRefreshTokenRepository>();
        refreshRepo.Setup(r => r.GetByTokenAsync(It.IsAny<string>())).ReturnsAsync((RefreshToken?)null);
        var service = CreateService(userManager, signInManager, refreshRepo);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.RefreshAsync("invalid"));
    }

    [Fact]
    public async Task RefreshAsync_ExpiredToken_ThrowsUnauthorized()
    {
        var userManager = GetUserManagerMock();
        var signInManager = GetSignInManagerMock(userManager.Object);
        var refreshRepo = new Mock<IRefreshTokenRepository>();
        var user = new AppUser { Id = "1" };
        var token = "token";
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        var stored = new RefreshToken
        {
            TokenHash = tokenHash,
            UserId = user.Id,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        };
        refreshRepo.Setup(r => r.GetByTokenAsync(tokenHash)).ReturnsAsync(stored);
        var service = CreateService(userManager, signInManager, refreshRepo);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.RefreshAsync(token));
    }
}

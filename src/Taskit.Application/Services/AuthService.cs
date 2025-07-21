using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

using Taskit.Application.DTOs;
using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Ardalis.GuardClauses;
using Taskit.Application.Common.Exceptions;

namespace Taskit.Application.Services;

public class AuthService(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IConfiguration configuration,
    IRefreshTokenRepository refreshTokenRepository,
    IHttpContextAccessor httpContextAccessor)
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly SignInManager<AppUser> _signInManager = signInManager;
    private readonly IConfiguration _configuration = configuration;
    private readonly IRefreshTokenRepository _refreshTokens = refreshTokenRepository;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task RegisterAsync(RegisterRequest dto)
    {
        var user = new AppUser { UserName = dto.Username, Email = dto.Email, FullName = dto.FullName };
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            throw new ValidationException(result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description }));
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        Guard.Against.Null(user, nameof(user), "Invalid credentials");

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded)
            throw new UnauthorizedAccessException();

        var token = GenerateJwtToken(user);
        var refreshToken = await CreateRefreshTokenAsync(user);

        var http = _httpContextAccessor.HttpContext;
        http?.Response.Cookies.Append(
            "refreshToken",
            refreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

        var response = new LoginResponse
        {
            AccessToken = token,
            RefreshToken = http?.Request.Headers.TryGetValue("X-No-Cookie", out var noCookie) == true && noCookie == "true" ? refreshToken : null,
            User = new AppUserDto
            {
                UserName = user.UserName!,
                Email = user.Email!,
                FullName = user.FullName!
            }
        };

        return response;
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
        _httpContextAccessor.HttpContext?.Response.Cookies.Delete("refreshToken");
    }

    public async Task<RefreshResponse> RefreshAsync(string? providedRefreshToken)
    {
        var http = _httpContextAccessor.HttpContext;
        var refreshToken = providedRefreshToken ?? http?.Request.Cookies["refreshToken"];
        Guard.Against.NullOrEmpty(refreshToken, nameof(refreshToken), "Invalid refresh token");

        var tokenHash = ComputeSha256Hash(refreshToken);
        var stored = await _refreshTokens.GetByTokenAsync(tokenHash);
        if (stored == null || stored.RevokedAt != null || stored.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedAccessException();

        stored.RevokedAt = DateTime.UtcNow;
        await _refreshTokens.UpdateAsync(stored);

        var newRefreshToken = await CreateRefreshTokenAsync(stored.User!);
        var token = GenerateJwtToken(stored.User!);

        http?.Response.Cookies.Append(
            "refreshToken",
            newRefreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

        var response = new RefreshResponse
        {
            AccessToken = token,
            RefreshToken = providedRefreshToken is not null || (http?.Request.Headers.TryGetValue("X-No-Cookie", out var noCookie) == true && noCookie == "true")
                ? newRefreshToken
                : null
        };

        return response;
    }

    private string GenerateJwtToken(AppUser user)
    {
        var jwtKey = _configuration["JWT:Key"];
        var jwtAudience = _configuration["JWT:Audience"];
        var jwtIssuer = _configuration["JWT:Issuer"];

        if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtAudience) || string.IsNullOrEmpty(jwtIssuer))
        {
            throw new InvalidOperationException("JWT settings are not configured");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id!),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> CreateRefreshTokenAsync(AppUser user)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var tokenHash = ComputeSha256Hash(token);

        var refresh = new RefreshToken
        {
            TokenHash = tokenHash,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            UserAgent = _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString(),
            IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress
        };

        await _refreshTokens.AddAsync(refresh);
        return token;
    }

    private static string ComputeSha256Hash(string raw)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes);
    }
}

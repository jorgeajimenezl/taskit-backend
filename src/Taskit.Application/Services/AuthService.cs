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
using Microsoft.Extensions.Options;
using Taskit.Application.Common.Settings;
using System.Net;

namespace Taskit.Application.Services;

public class AuthService(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IOptions<JwtSettings> jwtSettings,
    IRefreshTokenRepository refreshTokenRepository)
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly SignInManager<AppUser> _signInManager = signInManager;
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    private readonly IRefreshTokenRepository _refreshTokens = refreshTokenRepository;

    public async Task RegisterAsync(RegisterRequest dto)
    {
        var user = new AppUser { UserName = dto.Username, Email = dto.Email, FullName = dto.FullName };
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            throw new ValidationException(result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description }));
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest dto, string? userAgent = null, IPAddress? ipAddress = null)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email) ?? throw new UnauthorizedAccessException();
        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded)
            throw new UnauthorizedAccessException("Invalid email or password");

        var token = GenerateJwtToken(user);
        var refreshToken = await CreateRefreshTokenAsync(user, userAgent, ipAddress);

        var response = new LoginResponse
        {
            AccessToken = token,
            RefreshToken = refreshToken,
            User = new UserDto
            {
                Id = user.Id,
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
    }

    public async Task<RefreshResponse> RefreshAsync(string refreshToken, string? userAgent = null, IPAddress? ipAddress = null)
    {
        if (string.IsNullOrEmpty(refreshToken))
            throw new UnauthorizedAccessException("Refresh token is required");

        var tokenHash = ComputeSha256Hash(refreshToken);
        var stored = await _refreshTokens.GetByTokenAsync(tokenHash);
        if (stored == null || stored.RevokedAt != null || stored.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedAccessException();

        stored.RevokedAt = DateTime.UtcNow;
        await _refreshTokens.UpdateAsync(stored);

        var newRefreshToken = await CreateRefreshTokenAsync(stored.User!, userAgent, ipAddress);
        var token = GenerateJwtToken(stored.User!);

        var response = new RefreshResponse
        {
            AccessToken = token,
            RefreshToken = newRefreshToken
        };

        return response;
    }

    private string GenerateJwtToken(AppUser user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
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
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> CreateRefreshTokenAsync(AppUser user, string? userAgent = null, IPAddress? ipAddress = null)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var tokenHash = ComputeSha256Hash(token);

        var refresh = new RefreshToken
        {
            TokenHash = tokenHash,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            UserAgent = userAgent,
            IpAddress = ipAddress,
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

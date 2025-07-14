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

namespace Taskit.Application.Services;

public class AuthService(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IConfiguration configuration,
    IRefreshTokenRepository refreshTokenRepository)
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly SignInManager<AppUser> _signInManager = signInManager;
    private readonly IConfiguration _configuration = configuration;
    private readonly IRefreshTokenRepository _refreshTokens = refreshTokenRepository;

    public async Task RegisterAsync(RegisterRequest dto)
    {
        var user = new AppUser { UserName = dto.Username, Email = dto.Email, FullName = dto.FullName };
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(";", result.Errors.Select(e => e.Description)));
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return null;

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded)
            return null;

        var token = GenerateJwtToken(user);
        var refresh = await CreateRefreshTokenAsync(user);
        return new LoginResponse { Token = token, RefreshToken = refresh.Token };
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<RefreshResponse?> RefreshAsync(RefreshRequest dto)
    {
        var stored = await _refreshTokens.GetByTokenAsync(dto.RefreshToken);
        if (stored == null || stored.RevokedAt != null || stored.ExpiresAt <= DateTime.UtcNow)
            return null;

        stored.RevokedAt = DateTime.UtcNow;
        await _refreshTokens.UpdateAsync(stored);

        var newRefresh = await CreateRefreshTokenAsync(stored.User!);
        var token = GenerateJwtToken(stored.User!);
        return new RefreshResponse { Token = token, RefreshToken = newRefresh.Token };
    }

    private string GenerateJwtToken(IdentityUser user)
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
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<RefreshToken> CreateRefreshTokenAsync(AppUser user)
    {
        var refresh = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _refreshTokens.AddAsync(refresh);
        return refresh;
    }
}

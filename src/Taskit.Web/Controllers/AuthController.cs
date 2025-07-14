using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Taskit.Application.DTOs;
using Taskit.Domain.Entities;
using Taskit.Infrastructure;

namespace Taskit.Web.Controllers;

public class AuthController(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IConfiguration configuration,
    AppDbContext dbContext) : ApiControllerBase
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly SignInManager<AppUser> _signInManager = signInManager;
    private readonly IConfiguration _configuration = configuration;
    private readonly AppDbContext _db = dbContext;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest dto)
    {
        var user = new AppUser { UserName = dto.Username, Email = dto.Email };
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        return Ok();
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return Unauthorized();

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (result.Succeeded)
        {
            var token = GenerateJwtToken(user);
            var refresh = CreateRefreshToken(user);
            await _db.SaveChangesAsync();
            return Ok(new LoginResponse
            {
                Token = token,
                RefreshToken = refresh.Token
            });
        }

        return Unauthorized();
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok();
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<RefreshResponse>> Refresh(RefreshRequest dto)
    {
        var stored = await _db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == dto.RefreshToken && r.RevokedAt == null);

        if (stored == null || stored.ExpiresAt <= DateTime.UtcNow)
            return Unauthorized();

        stored.RevokedAt = DateTime.UtcNow;
        var newRefresh = CreateRefreshToken(stored.User!);
        await _db.SaveChangesAsync();

        var token = GenerateJwtToken(stored.User!);
        return Ok(new RefreshResponse { Token = token, RefreshToken = newRefresh.Token });
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

    private RefreshToken CreateRefreshToken(AppUser user)
    {
        var refresh = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _db.RefreshTokens.Add(refresh);
        return refresh;
    }
}
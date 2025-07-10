using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Taskit.Domain.DTOs;
using Taskit.Domain.Entities;

namespace Taskit.Web.Controllers;

public class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IConfiguration configuration) : ApiControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IConfiguration _configuration = configuration;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest dto)
    {
        var user = new ApplicationUser { UserName = dto.Username, Email = dto.Email };
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
            return Ok(new LoginResponse
            {
                Token = token,
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
}
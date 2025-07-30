using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Taskit.Application.Common.Settings;
using Taskit.Application.DTOs;
using Taskit.Application.Services;

namespace Taskit.Web.Controllers;

public class AuthController(AuthService authService, IOptions<JwtSettings> jwtSettings) : ApiControllerBase
{
    private readonly AuthService _auth = authService;
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    private CookieOptions RefreshCookieOptions => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Expires = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
    };

    [HttpPost("register", Name = "RegisterUser")]
    public async Task<IActionResult> Register(RegisterRequest dto)
    {
        await _auth.RegisterAsync(dto);
        return Ok();
    }

    [HttpPost("login", Name = "LoginUser")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest dto)
    {
        var userAgent = Request.Headers.UserAgent.ToString();
        var ipAddress = Request.HttpContext.Connection.RemoteIpAddress;
        var result = await _auth.LoginAsync(dto, userAgent, ipAddress);

        if (!Request.Headers.TryGetValue("X-No-Cookie", out var noCookie) || noCookie != "true")
        {
            Response.Cookies.Append(
                "refreshToken",
                result.RefreshToken!,
                RefreshCookieOptions
            );
        }

        return Ok(new LoginResponse
        {
            AccessToken = result.AccessToken,
            RefreshToken = noCookie == "true" ? null : result.RefreshToken,
            User = result.User
        });
    }

    [Authorize]
    [HttpPost("logout", Name = "LogoutUser")]
    public async Task<IActionResult> Logout()
    {
        await _auth.LogoutAsync();
        Response.Cookies.Delete("refreshToken");
        return Ok();
    }

    [HttpPost("refresh", Name = "RefreshToken")]
    public async Task<ActionResult<RefreshResponse>> Refresh([FromBody] RefreshRequest? dto)
    {
        var refreshToken = Request.Cookies["refreshToken"] ?? dto?.RefreshToken;
        if (string.IsNullOrEmpty(refreshToken))
            throw new UnauthorizedAccessException("Refresh token is required");

        var userAgent = Request.Headers.UserAgent.ToString();
        var ipAddress = Request.HttpContext.Connection.RemoteIpAddress;

        var result = await _auth.RefreshAsync(refreshToken, userAgent, ipAddress);

        if (!Request.Headers.TryGetValue("X-No-Cookie", out var noCookie) || noCookie != "true")
        {
            Response.Cookies.Append(
                "refreshToken",
                result.RefreshToken!,
                RefreshCookieOptions
            );
        }

        return Ok(new RefreshResponse
        {
            AccessToken = result.AccessToken,
            RefreshToken = noCookie == "true" ? null : result.RefreshToken
        });
    }
}

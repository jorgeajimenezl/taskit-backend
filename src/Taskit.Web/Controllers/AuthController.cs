using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Taskit.Application.Common.Settings;
using Taskit.Application.DTOs;
using Taskit.Application.Services;
using System.Threading;

namespace Taskit.Web.Controllers;

public class AuthController(AuthService authService, IOptions<JwtSettings> jwtSettings) : ApiControllerBase
{
    private readonly AuthService _auth = authService;
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    private CookieOptions AccessCookieOptions => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Expires = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes)
    };

    private CookieOptions RefreshCookieOptions => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Expires = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
    };

    [HttpPost("register", Name = "RegisterUser")]
    public async Task<IActionResult> Register(RegisterRequest dto, CancellationToken cancellationToken)
    {
        await _auth.RegisterAsync(dto, cancellationToken);
        return Ok();
    }

    [HttpPost("login", Name = "LoginUser")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest dto, CancellationToken cancellationToken)
    {
        var userAgent = Request.Headers.UserAgent.ToString();
        var ipAddress = Request.HttpContext.Connection.RemoteIpAddress;
        var result = await _auth.LoginAsync(dto, userAgent, ipAddress, cancellationToken);

        if (!Request.Headers.TryGetValue("X-No-Cookie", out var noCookie) || noCookie != "true")
        {
            Response.Cookies.Append(
                "accessToken",
                result.AccessToken!,
                AccessCookieOptions
            );
            Response.Cookies.Append(
                "refreshToken",
                result.RefreshToken!,
                RefreshCookieOptions
            );
        }

        return Ok(new LoginResponse
        {
            AccessToken = noCookie == "true" ? result.AccessToken : null,
            RefreshToken = noCookie == "true" ? result.RefreshToken : null,
            User = result.User
        });
    }

    [Authorize]
    [HttpPost("logout", Name = "LogoutUser")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await _auth.LogoutAsync(cancellationToken);
        Response.Cookies.Delete("accessToken");
        Response.Cookies.Delete("refreshToken");
        return Ok();
    }

    [HttpPost("refresh", Name = "RefreshToken")]
    public async Task<ActionResult<RefreshResponse>> Refresh([FromBody] RefreshRequest? dto, CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies["refreshToken"] ?? dto?.RefreshToken;
        if (string.IsNullOrEmpty(refreshToken))
            throw new UnauthorizedAccessException("Refresh token is required");

        var userAgent = Request.Headers.UserAgent.ToString();
        var ipAddress = Request.HttpContext.Connection.RemoteIpAddress;

        var result = await _auth.RefreshAsync(refreshToken, userAgent, ipAddress, cancellationToken);

        if (!Request.Headers.TryGetValue("X-No-Cookie", out var noCookie) || noCookie != "true")
        {
            Response.Cookies.Append(
                "accessToken",
                result.AccessToken!,
                AccessCookieOptions
            );
            Response.Cookies.Append(
                "refreshToken",
                result.RefreshToken!,
                RefreshCookieOptions
            );
        }

        return Ok(new RefreshResponse
        {
            AccessToken = noCookie == "true" ? result.AccessToken : null,
            RefreshToken = noCookie == "true" ? result.RefreshToken : null
        });
    }
}

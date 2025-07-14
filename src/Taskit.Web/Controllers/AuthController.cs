using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskit.Application.DTOs;
using Taskit.Application.Services;

namespace Taskit.Web.Controllers;

public class AuthController(AuthService authService) : ApiControllerBase
{
    private readonly AuthService _auth = authService;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest dto)
    {
        await _auth.RegisterAsync(dto);
        return Ok();
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest dto)
    {
        var result = await _auth.LoginAsync(dto);
        return result is null ? Unauthorized() : Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _auth.LogoutAsync();
        return Ok();
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<RefreshResponse>> Refresh(RefreshRequest dto)
    {
        var result = await _auth.RefreshAsync(dto);
        return result is null ? Unauthorized() : Ok(result);
    }
}

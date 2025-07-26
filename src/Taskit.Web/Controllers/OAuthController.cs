using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Client.AspNetCore;
using Taskit.Application.Common.Exceptions;
using Taskit.Application.DTOs;
using Taskit.Application.Services;

namespace Taskit.Web.Controllers;

public class OAuthController(AuthService authService) : ApiControllerBase
{
    private readonly AuthService _authService = authService;
    private static readonly Dictionary<string, string> ProvidersMapping = new()
    {
        ["github"] = "GitHub",
    };

    private static string GetProviderName(string provider)
    {
        if (ProvidersMapping.TryGetValue(provider.ToLowerInvariant(), out var providerName))
            return providerName;

        throw new ValidationException(new Dictionary<string, string[]>
        {
            ["provider"] = ["Invalid provider specified."]
        });
    }

    [HttpGet("login/{provider}")]
    public IActionResult Login(string provider)
    {
        var providerName = GetProviderName(provider);

        var properties = new AuthenticationProperties(new Dictionary<string, string?>
        {
            [OpenIddictClientAspNetCoreConstants.Properties.ProviderName] = providerName,
        })
        {
            RedirectUri = Url.Action(nameof(ExternalLoginCallback), providerName)
        };

        return Challenge(properties, OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpGet("callback/{provider}")]
    public async Task<ActionResult<LoginResponse>> ExternalLoginCallback(string provider)
    {
        var result = await HttpContext.AuthenticateAsync(OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);

        if (!result.Succeeded || result?.Principal is null)
            // TODO: create a custom exception for authentication failures
            return BadRequest("Authentication failed.");

        if (result?.Principal is not ClaimsPrincipal principal)
            return BadRequest();

        var providerUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(providerUserId))
            return BadRequest();

        var providerName = GetProviderName(provider);
        var loginResponse = await _authService.ExternalLoginAsync(providerName, providerUserId);
        return Ok(loginResponse);
    }

    [Authorize]
    [HttpGet("disconnect/{provider}")]
    public async Task<IActionResult> Disconnect(string provider)
    {
        var providerName = GetProviderName(provider);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        await _authService.DisconnectExternalLoginAsync(userId!, providerName);
        return NoContent();
    }
}
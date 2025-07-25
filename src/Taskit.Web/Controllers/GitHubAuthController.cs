using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Client.AspNetCore;
using static OpenIddict.Client.AspNetCore.OpenIddictClientAspNetCoreConstants;
using Taskit.Web.GitHub;

namespace Taskit.Web.Controllers;

public class GitHubAuthController(GitHubAuthService github) : ApiControllerBase
{
    private readonly GitHubAuthService _github = github;

    [HttpGet("github/login")]
    public IActionResult Login()
    {
        var properties = new AuthenticationProperties(new Dictionary<string, string?>
        {
            [Properties.ProviderName] = "GitHub"
        })
        {
            RedirectUri = Url.Action(nameof(Callback))
        };

        return Challenge(properties, OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpGet("github/callback")]
    public async Task<ActionResult<GitHubProfile>> Callback()
    {
        var result = await HttpContext.AuthenticateAsync(OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);
        if (result is not { Succeeded: true })
            return Forbid();

        var token = result.Properties.GetTokenValue(Tokens.BackchannelAccessToken);
        if (string.IsNullOrEmpty(token))
            return Forbid();

        var profile = await _github.GetProfileAsync(token);
        return Ok(profile);
    }
}

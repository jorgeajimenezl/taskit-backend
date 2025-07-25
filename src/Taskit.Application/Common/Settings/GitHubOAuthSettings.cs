namespace Taskit.Application.Common.Settings;

public class GitHubOAuthSettings
{
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
    public required string RedirectUri { get; set; }
}

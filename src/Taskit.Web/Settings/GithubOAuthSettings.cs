namespace Taskit.Web.Settings;

public class GithubOAuthSettings
{
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
    public required string RedirectUri { get; set; }
}
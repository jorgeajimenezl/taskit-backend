using System.Net.Http.Headers;
using System.Text.Json;

namespace Taskit.Web.GitHub;

public class GitHubAuthService(HttpClient client)
{
    private readonly HttpClient _client = client;

    public async Task<GitHubProfile> GetProfileAsync(string accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.UserAgent.ParseAdd("Taskit-App");

        using var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        var profile = await JsonSerializer.DeserializeAsync<GitHubProfile>(stream);
        return profile ?? new GitHubProfile();
    }
}

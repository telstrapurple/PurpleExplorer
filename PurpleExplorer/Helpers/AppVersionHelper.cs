using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PurpleExplorer.Helpers;

public static class AppVersionHelper
{
    public static async Task<GithubRelease> GetLatestRelease()
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Application");
        var response =
            await httpClient.GetAsync(
                "https://api.github.com/repos/telstrapurple/PurpleExplorer/releases/latest");
        var content = await response.Content.ReadAsStringAsync();
        var githubRelease = JsonConvert.DeserializeObject<GithubRelease>(content);
        return githubRelease;
    }
}

public class GithubRelease
{
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("url")]
    public string Url { get; set; }
    [JsonProperty("html_url")]
    public string HtmlUrl { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }
    [JsonProperty("published_at")]
    public DateTime PublishedAt { get; set; } 
}
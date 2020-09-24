using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PurpleExplorer.Services
{
    public static class AppVersionHelper
    {
        public static async Task<GithubRelease> GetLatestRelease()
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Application");
                var response =
                    await httpClient.GetAsync(
                        "https://api.github.com/repos/telstrapurple/PurpleExplorer/releases/latest");
                var content = await response.Content.ReadAsStringAsync();
                var githubRelease = JsonConvert.DeserializeObject<GithubRelease>(content);
                return githubRelease;
            }
        }
    }

    public class GithubRelease
    {
        public int id { get; set; } 
        public string url { get; set; } 
        public string html_url { get; set; }
        public string tag_name { get; set; } 
        public string name { get; set; } 
        public DateTime created_at { get; set; } 
        public DateTime published_at { get; set; } 
        public string body { get; set; } 
    }
}
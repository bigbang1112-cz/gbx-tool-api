using System.Text.Json.Serialization;

namespace GbxToolAPI.CLI;

public class GitHubRelease
{
    [JsonPropertyName("tag_name")]
    public string TagName { get; set; } = "";

    [JsonPropertyName("published_at")]
    public DateTimeOffset PublishedAt { get; set; }
}

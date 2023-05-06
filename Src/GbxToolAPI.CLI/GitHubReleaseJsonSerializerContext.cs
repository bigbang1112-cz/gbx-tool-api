using System.Text.Json.Serialization;

namespace GbxToolAPI.CLI;

[JsonSerializable(typeof(GitHubRelease))]
public partial class GitHubReleaseJsonSerializerContext : JsonSerializerContext
{
}

namespace GbxToolAPI.Server.Options;

public class DiscordOptions
{
    public string OwnerSnowflake { get; set; } = "";
    public DiscordClientOptions Client { get; set; } = new();
}

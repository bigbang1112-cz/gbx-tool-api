namespace GbxToolAPI.Client.Services;

public interface IDynamicScriptService
{
    Task SpawnScriptAsync(string src, string id);
    Task DespawnScriptAsync(string id);
}

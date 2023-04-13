using Microsoft.AspNetCore.Components;

namespace GbxToolAPI.Client;

public abstract class ToolComponentBase<T> : ComponentBase where T : class, ITool
{
    private string selectedConfigName = "Default";

    [Parameter]
    [EditorRequired]
    public required string Route { get; set; }

    public string SelectedConfigName
    {
        get => selectedConfigName;
        set
        {
            selectedConfigName = value;
            SyncLocalStorage.SetItem($"Tool:{Route}:Config:Selected", selectedConfigName);
        }
    }

    [Parameter]
    [EditorRequired]
    public required Dictionary<string, Config> Configs { get; set; } = new();

    [Inject]
    public required Blazored.LocalStorage.ISyncLocalStorageService SyncLocalStorage { get; set; }

    public ToolComponentBase()
    {
        
    }
}

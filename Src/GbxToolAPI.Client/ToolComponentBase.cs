﻿using GbxToolAPI.Client.Models;
using GbxToolAPI.Client.Models.UtilImport;
using Microsoft.AspNetCore.Components;

namespace GbxToolAPI.Client;

public abstract class ToolComponentBase<T> : ComponentBase where T : class, ITool
{
    [Inject]
    public required Blazored.LocalStorage.ISyncLocalStorageService SyncLocalStorage { get; set; }

    [Inject]
    public required Blazored.LocalStorage.ILocalStorageService LocalStorage { get; set; }

    [Parameter]
    [EditorRequired]
    public required string Route { get; set; }

    public string SelectedConfigName
    {
        get
        {
            var val = SyncLocalStorage.GetItemAsString($"Tool:{Route}:Config:Selected");

            if (string.IsNullOrEmpty(val))
            {
                return "Default";
            }

            return val;
        }
        set
        {
            SyncLocalStorage.SetItem($"Tool:{Route}:Config:Selected", value);
        }
    }

    [Parameter]
    [EditorRequired]
    public required Dictionary<string, Config> Configs { get; set; } = new();

    [Parameter]
    [EditorRequired]
    public HashSet<GbxModel> GbxSelection { get; set; } = new();

    [Parameter]
    [EditorRequired]
    public IEnumerable<UtilImportType> ImportTypes { get; set; } = Enumerable.Empty<UtilImportType>();

    public ToolComponentBase()
    {
        
    }

    public void UpdateConfig()
    {
        SyncLocalStorage.SetItem($"Tool:{Route}:Config", Configs.ToDictionary(x => x.Key, x => (object)x.Value));
    }

    public async Task UpdateConfigAsync()
    {
        await LocalStorage.SetItemAsync($"Tool:{Route}:Config", Configs.ToDictionary(x => x.Key, x => (object)x.Value));
    }
}

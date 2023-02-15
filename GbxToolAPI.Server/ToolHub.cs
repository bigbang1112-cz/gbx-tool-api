using Microsoft.AspNetCore.SignalR;

namespace GbxToolAPI.Server;

public abstract class ToolHub
{
    private readonly Hub hub;

    public IHubCallerClients Clients => hub.Clients;
    public HubCallerContext Context => hub.Context;

    protected ToolHub(Hub hub)
    {
        this.hub = hub;
    }
}

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace GbxToolAPI.Client;

public abstract class ToolHub : IAsyncDisposable
{
    private readonly ILogger<ToolHub>? logger;

    protected HubConnection Connection { get; }

    public bool Started { get; private set; }
    public string? ConnectionId => Connection.ConnectionId;
    public HubConnectionState State => Connection.State;

	public ToolHub(string baseAddress, ILogger<ToolHub>? logger = null)
    {
        this.logger = logger;

        if (!baseAddress.EndsWith('/'))
        {
            baseAddress += '/';
        }

        var type = GetType();
        var hubAddress = baseAddress + type.Name.ToLower();

        Connection = new HubConnectionBuilder()
            .WithUrl(hubAddress)
            .WithAutomaticReconnect()
            .Build();
    }

    public async Task<bool> StartAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Connection.StartAsync(cancellationToken);
            return Started = true;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to start hub connection");
            return Started = false;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await Connection.StopAsync(cancellationToken);
        Started = false;
    }

    public async ValueTask DisposeAsync()
    {
        await Connection.DisposeAsync();
    }
}

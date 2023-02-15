using Microsoft.Extensions.Logging;

namespace GbxToolAPI.Client.Tests;

internal class TestHub : ToolHub
{
    public event Func<Task>? Pong;

    public partial Task Ping();

    public TestHub(string baseAddress, ILogger<TestHub>? logger = null) : base(baseAddress, logger)
    {

    }
}

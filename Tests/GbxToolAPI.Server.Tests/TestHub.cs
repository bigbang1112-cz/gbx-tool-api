namespace GbxToolAPI.Server.Tests;

public class TestHub
{
    public event Func<Task>? Ping;

    public partial Task Pong();
}

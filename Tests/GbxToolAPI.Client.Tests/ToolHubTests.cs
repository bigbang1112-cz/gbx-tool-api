namespace GbxToolAPI.Client.Tests;

public class ToolHubTests
{
    [Fact]
    public void Constructor()
    {
        var c = new TestHub("http://localhost:5000/");
        
    }
}

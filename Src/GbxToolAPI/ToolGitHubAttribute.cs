namespace GbxToolAPI;

[AttributeUsage(AttributeTargets.Class)]
public class ToolGitHubAttribute : Attribute
{
    public string Repository { get; }
    
    public ToolGitHubAttribute(string repository)
    {
        Repository = repository;
    }
}

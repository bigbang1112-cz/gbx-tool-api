namespace GbxToolAPI;

[AttributeUsage(AttributeTargets.Class)]
public class ToolRootUrlAttribute : Attribute
{
    public string RootUrl { get; }

    public ToolRootUrlAttribute(string rootUrl)
	{
        RootUrl = rootUrl;
    }
}

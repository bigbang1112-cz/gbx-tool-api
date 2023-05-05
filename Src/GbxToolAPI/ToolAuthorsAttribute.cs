namespace GbxToolAPI;

[AttributeUsage(AttributeTargets.Class)]
public class ToolAuthorsAttribute : Attribute
{
    public string Authors { get; }

    public ToolAuthorsAttribute(string authors)
    {
        Authors = authors;
    }
}

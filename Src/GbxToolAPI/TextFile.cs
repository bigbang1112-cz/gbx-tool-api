namespace GbxToolAPI;

public class TextFile
{
    public string Text { get; }

    public TextFile(string text)
    {
        Text = text;
    }
    
    public static explicit operator string(TextFile textFile)
    {
        return textFile.Text;
    }
}

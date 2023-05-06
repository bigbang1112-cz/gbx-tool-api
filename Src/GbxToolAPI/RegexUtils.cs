using System.Text.RegularExpressions;

namespace GbxToolAPI;

public partial class RegexUtils
{
    [GeneratedRegex("(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z0-9])")]
    private static partial Regex RegexPascalCaseToKebabCase();
    
    [GeneratedRegex(@"[^\u0000-\u00FF]")]
    private static partial Regex RegexExtendedAsciiValid();

    public static string PascalCaseToKebabCase(string str)
    {
        return RegexPascalCaseToKebabCase().Replace(str, "-$1").Trim().ToLower();
    }

    public static string GetExtendedAsciiValid(string str)
    {
        return RegexExtendedAsciiValid().Replace(str, "_");
    }

}

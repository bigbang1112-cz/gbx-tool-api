using System.Reflection;

namespace GbxToolAPI;

public static class AssetsManager<TTool> where TTool : ITool
{
    public static T GetFromYml<T>(string path)
    {
        var entryAss = Assembly.GetEntryAssembly();

        var runsViaConsole = entryAss?.GetReferencedAssemblies()?
            .Any(x => x.Name == "GbxToolAPI.Console") ?? false;

        if (!runsViaConsole)
        {
            throw new NotSupportedException("This method is only supported when running via GbxToolAPI.Console");
        }
        
        return GetFromYmlOnDisk<T>(path, entryAss);
    }

    private static T GetFromYmlOnDisk<T>(string path, Assembly? entryAss)
    {
        var rootPath = entryAss?.Location is null ? "" : Path.GetDirectoryName(entryAss.Location) ?? "";

        var toolAssetsIdentifier = typeof(TTool).GetCustomAttribute<ToolAssetsAttribute>()?.Identifier;

        if (toolAssetsIdentifier is null)
        {
            throw new NotSupportedException($"Assets are not supported for {typeof(TTool).Name}");
        }
        
        using var r = File.OpenText(Path.Combine(rootPath, "Assets", "Tools", toolAssetsIdentifier, path));
        
        return Yml.Deserializer.Deserialize<T>(r);
    }
}

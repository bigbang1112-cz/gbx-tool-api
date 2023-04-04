using System.Reflection;
using System.Text;

namespace GbxToolAPI;

public static class AssetsManager<TTool> where TTool : ITool
{
    private static Func<string, byte[]>? ExternalRetrieve { get; set; }

    public static T GetFromYml<T>(string path)
    {
        var entryAss = Assembly.GetEntryAssembly();

        var runsViaConsole = entryAss?.GetReferencedAssemblies()?
            .Any(x => x.Name == "GbxToolAPI.Console") ?? false;

        if (!runsViaConsole)
        {
            if (ExternalRetrieve is null)
            {
                throw new Exception("ExternalRetrieve needs to be set.");
            }

            var data = ExternalRetrieve(path);
            var str = Encoding.UTF8.GetString(data);

            return Yml.Deserializer.Deserialize<T>(str);
        }

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

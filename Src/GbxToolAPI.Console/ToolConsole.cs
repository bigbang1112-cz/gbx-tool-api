using GBX.NET;
using System.Reflection;

namespace GbxToolAPI.Console;

public class ToolConsole<T> where T : ITool
{
    public static Task<ToolConsole<T>> CreateAsync(string[] args)
    {
        var console = new ToolConsole<T>();

        var interfaces = typeof(T).GetInterfaces();

        var configTypes = new List<Type>();

        foreach (var iface in interfaces)
        {
            switch (iface.Name)
            {
                case "IConfigurable":
                    if (!iface.IsGenericType) break;
                    configTypes.Add(iface.GetGenericArguments()[0]);
                    break;
            }
        }

        var configProps = new Dictionary<string, PropertyInfo>();

        foreach (var configType in configTypes)
        {
            foreach (var prop in configType.GetProperties())
            {
                var nameLower = prop.Name.ToLowerInvariant();

                configProps[$"-c:{nameLower}"] = prop;
                configProps[$"-c:{configType.Name.ToLowerInvariant()}:{nameLower}"] = prop;
            }
        }

        var ctors = typeof(T).GetConstructors();

        var filesChecked = false;

        foreach (var arg in args)
        {
            if (!filesChecked)
            {
                var node = GameBox.ParseNode(arg);
            }

            var argLower = arg.ToLowerInvariant();

            if (configProps.TryGetValue(argLower, out var confProp))
            {
                filesChecked = true;
            }
        }

        return Task.FromResult(console);
    }
}

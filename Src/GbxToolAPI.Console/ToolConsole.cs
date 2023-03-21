using GBX.NET;
using System.Collections;
using System.Reflection;

namespace GbxToolAPI.Console;

public class ToolConsole<T> where T : ITool
{
    public static Task<ToolConsole<T>> CreateAsync(string[] args)
    {
        var console = new ToolConsole<T>();

        var configProps = GetConfigProps();
        var potentialCtors = typeof(T).GetConstructors().ToList();
        var paramDict = new Dictionary<ParameterInfo, object>();

        var filesChecked = false;

        foreach (var arg in args)
        {
            if (!filesChecked)
            {
                var node = GameBox.ParseNode(arg) ?? throw new Exception();
                var nodeType = node.GetType();

                var updatedPotentialCtors = new List<ConstructorInfo>();

                foreach (var ctor in potentialCtors)
                {
                    foreach (var p in ctor.GetParameters())
                    {
                        if (p.ParameterType.IsGenericType && p.ParameterType.IsAssignableTo(typeof(IEnumerable)))
                        {
                            var enumerableType = p.ParameterType.GetGenericArguments()[0];

                            if (enumerableType.IsAssignableTo(nodeType))
                            {
                                updatedPotentialCtors.Add(ctor);
                                paramDict[p] = new[] { node };
                            }
                        }
                        else if (p.ParameterType.IsAssignableTo(nodeType))
                        {
                            updatedPotentialCtors.Add(ctor);
                            paramDict[p] = node;
                        }
                    }
                }

                potentialCtors = updatedPotentialCtors;
            }

            var argLower = arg.ToLowerInvariant();

            if (configProps.TryGetValue(argLower, out var confProp))
            {
                filesChecked = true;
            }
        }

        return Task.FromResult(console);
    }

    private static Dictionary<string, PropertyInfo> GetConfigProps()
    {
        var interfaces = typeof(T).GetInterfaces();

        var configTypes = new List<Type>();

        foreach (var iface in interfaces)
        {
            switch (iface.Name)
            {
                case "IConfigurable`1":
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
                configProps.Add($"-c:{configType.Name.ToLowerInvariant()}:{nameLower}", prop);
            }
        }

        return configProps;
    }
}

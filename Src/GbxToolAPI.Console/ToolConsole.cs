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

        var inputFiles = args.TakeWhile(arg => !arg.StartsWith("-")).ToArray();
        var ctorArgs = inputFiles.Select(file => GameBox.ParseNode(file) ?? throw new Exception()).ToArray();
        var remainingArgs = args.Skip(inputFiles.Length).ToArray();

        var toolCtor = GetSuitableConstructor(ctorArgs);

        if (toolCtor != null)
        {
            var tool = toolCtor.Invoke(ctorArgs);
            // use the tool object as needed
        }

        foreach (var arg in remainingArgs)
        {
            var argLower = arg.ToLowerInvariant();

            if (configProps.TryGetValue(argLower, out var confProp))
            {

            }
        }

        return Task.FromResult(console);
    }

    private static ConstructorInfo? GetSuitableConstructor(Node[] ctorArgs)
    {
        foreach (var constructor in typeof(T).GetConstructors())
        {
            var parameters = constructor.GetParameters();

            if (parameters.Length != ctorArgs.Length) // Does not work well on enumerables
            {
                continue;
            }

            var isMatch = true;

            for (int i = 0; i < parameters.Length; i++)
            {
                var expectedType = parameters[i].ParameterType;
                var actualType = ctorArgs[i].GetType();

                if (!expectedType.IsAssignableFrom(actualType))
                {
                    isMatch = false;
                    break;
                }
            }

            if (isMatch)
            {
                return constructor;
            }
        }

        return null;
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

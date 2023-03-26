using GBX.NET;
using System.Collections;
using System.Reflection;

namespace GbxToolAPI.Console;

public class ToolConsole<T> where T : class, ITool
{
    public static Task<ToolConsole<T>> CreateAsync(string[] args)
    {
        var type = typeof(T);

        var outputInterfaces = type.GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHasOutput<>))
            .ToList();

        if (!outputInterfaces.Any())
        {
            throw new Exception("Tool must implement at least one IHasOutput<T> interface.");
        }

        var console = new ToolConsole<T>();

        var configProps = GetConfigProps();

        var inputFiles = args.TakeWhile(arg => !arg.StartsWith('-')).ToArray();
        var remainingArgs = args.Skip(inputFiles.Length);

        var singleOutput = false;

        foreach (var arg in remainingArgs)
        {
            var argLower = arg.ToLowerInvariant();

            switch (argLower)
            {
                case "-singleoutput": // Merge will produce only one instance of Tool
                    singleOutput = true;
                    break;
            }

            if (configProps.TryGetValue(argLower, out var confProp))
            {

            }
        }

        foreach (var toolInstance in ToolConstructorPicker.CreateInstances<T>(inputFiles, singleOutput))
        {
            foreach (var produceMethod in type.GetMethods().Where(m => m.Name == nameof(IHasOutput<object>.Produce)))
            {
                var output = produceMethod.Invoke(toolInstance, null);
                
                if (output is null)
                {
                    continue;
                }

                var outputSaver = new OutputSaver(output);
                outputSaver.Save();
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

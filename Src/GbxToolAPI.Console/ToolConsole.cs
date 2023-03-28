using System.Diagnostics;
using System.Reflection;
namespace GbxToolAPI.Console;

public class ToolConsole<T> where T : class, ITool
{
    public static Task<ToolConsole<T>> CreateAsync(string[] args)
    {
        System.Console.WriteLine("Initializing the console...");

        var rootPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
        var type = typeof(T);

        var outputInterfaces = type.GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHasOutput<>))
            .ToList();

        if (!outputInterfaces.Any())
        {
            throw new Exception("Tool must implement at least one IHasOutput<T> interface.");
        }

        var console = new ToolConsole<T>();

        System.Console.WriteLine("Using the tool: " + type.GetCustomAttribute<ToolNameAttribute>()?.Name);
        System.Console.WriteLine("Retrieving possible config options...");

        var configProps = GetConfigProps(out var configPropTypes);
        
        System.Console.WriteLine("Scanning all the command line arguments...");

        var inputFiles = args.TakeWhile(arg => !arg.StartsWith('-')).ToArray();

        System.Console.WriteLine();
        System.Console.WriteLine("Detected input files:\n- " + string.Join("\n- ", inputFiles.Select(Path.GetFileName)));

        var remainingArgs = args.Skip(inputFiles.Length);

        var singleOutput = false;
        var customConfig = default(string);
        var listConfigProps = new Dictionary<PropertyInfo, object?>();

        System.Console.WriteLine();
        System.Console.WriteLine("Additional arguments:");

        var argEnumerator = remainingArgs.GetEnumerator();

        while (argEnumerator.MoveNext())
        {
            var arg = argEnumerator.Current;
            var argLower = arg.ToLowerInvariant();

            switch (argLower)
            {
                case "-singleoutput": // Merge will produce only one instance of Tool
                    singleOutput = true;
                    continue;
                case "-config":
                    if (!argEnumerator.MoveNext())
                    {
                        throw new Exception("Missing string value for option " + arg);
                    }

                    customConfig = argEnumerator.Current;
                    continue;
            }

            if (!configProps.TryGetValue(argLower, out var confProp))
            {
                throw new Exception("Unknown argument: " + arg);
            }

            if (confProp.PropertyType == typeof(string))
            {
                if (!argEnumerator.MoveNext())
                {
                    throw new Exception("Missing value for config option " + arg);
                }

                var val = argEnumerator.Current;

                System.Console.WriteLine($": {arg} \"{val}\"");

                listConfigProps.Add(confProp, val);
            }
            else
            {
                throw new Exception("Config option " + arg + " is not a string.");
            }
        }

        var actualConfigs = new Dictionary<PropertyInfo, Config>();

        if (configPropTypes.Count > 0)
        {
            Directory.CreateDirectory((rootPath is null ? "" : rootPath + Path.DirectorySeparatorChar) + "Config");

            customConfig ??= "Default";

            foreach (var configProp in configPropTypes)
            {
                var configType = configProp.PropertyType;

                var fileName = (rootPath is null ? "" : rootPath + Path.DirectorySeparatorChar) + Path.Combine("Config", $"{customConfig}{(configPropTypes.Count > 1 ? $"_{configType.Name}" : "")}.yml");

                Config? config;

                if (File.Exists(fileName))
                {
                    using var r = new StreamReader(fileName);
                    config = (Config)Yml.Deserializer.Deserialize(r, configType)!;
                }
                else
                {
                    config = (Config)Activator.CreateInstance(configType)!;
                    File.WriteAllText(fileName, Yml.Serializer.Serialize(config));
                }

                actualConfigs.Add(configProp, config);
            }
        }


        System.Console.WriteLine();

        foreach (var toolInstance in ToolConstructorPicker.CreateInstances<T>(inputFiles, singleOutput))
        {
            System.Console.WriteLine("Creating tool instance...");

            foreach (var (configProp, config) in actualConfigs)
            {
                configProp.SetValue(toolInstance, config);

                foreach (var (prop, value) in listConfigProps)
                {
                    try
                    {
                        prop.SetValue(config, value);
                    }
                    catch (TargetInvocationException ex)
                    {
                        throw ex.InnerException ?? ex;
                    }
                }
            }

            foreach (var produceMethod in type.GetMethods().Where(m => m.Name == nameof(IHasOutput<object>.Produce)))
            {
                var typeName = GetTypeName(produceMethod.ReturnType);

                System.Console.WriteLine($"Producing {typeName}...");

                var watch = Stopwatch.StartNew();

                var output = produceMethod.Invoke(toolInstance, null);

                var name = output is null ? "[null]" : GetTypeName(output.GetType());

                System.Console.WriteLine($"Produced {name}. ({watch.Elapsed.TotalMilliseconds}ms)");

                if (output is null)
                {
                    continue;
                }

                var outputSaver = new OutputSaver(output);
                outputSaver.Save();
            }

            System.Console.WriteLine();
        }

        System.Console.WriteLine("Complete!");

        return Task.FromResult(console);
    }

    private static string GetTypeName(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }
        else
        {
            if (type.GetGenericTypeDefinition() == typeof(NodeFile<>))
            {
                return type.GetGenericArguments()[0].Name;
            }
        }

        return "[unknown]";
    }

    private static Dictionary<string, PropertyInfo> GetConfigProps(out IList<PropertyInfo> configs)
    {
        var interfaces = typeof(T).GetInterfaces();

        configs = new List<PropertyInfo>();

        foreach (var iface in interfaces)
        {
            switch (iface.Name)
            {
                case "IConfigurable`1":
                    if (!iface.IsGenericType) break;
                    configs.Add(iface.GetProperty(nameof(IConfigurable<Config>.Config)) ?? throw new UnreachableException("Config property not available"));
                    break;
            }
        }

        var configProps = new Dictionary<string, PropertyInfo>();

        foreach (var propConfig in configs)
        {
            foreach (var prop in propConfig.PropertyType.GetProperties())
            {
                var nameLower = prop.Name.ToLowerInvariant();

                configProps[$"-c:{nameLower}"] = prop;
                configProps.Add($"-c:{propConfig.PropertyType.Name.ToLowerInvariant()}:{nameLower}", prop);
            }
        }

        return configProps;
    }
}

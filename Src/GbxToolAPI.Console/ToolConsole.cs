using System.Diagnostics;
using System.Reflection;
namespace GbxToolAPI.Console;

public class ToolConsole<T> where T : class, ITool
{
    private static readonly string rootPath;

    public static async Task<ToolConsole<T>?> RunAsync(string[] args)
    {
        var c = default(ToolConsole<T>);

        try
        {
            c = await RunCanThrowAsync(args);
        }
        catch (ConsoleFailException ex)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine(ex.Message);
            System.Console.ResetColor();
            System.Console.Write("Press any key to continue...");
            System.Console.ReadKey();
        }
        catch (Exception ex)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine(ex);
            System.Console.ResetColor();
            System.Console.Write("Press any key to continue...");
            System.Console.ReadKey();
        }

        return c;
    }

    static ToolConsole()
    {
        GBX.NET.Lzo.SetLzo(typeof(GBX.NET.LZO.MiniLZO));
        
        rootPath = Environment.ProcessPath is null ? "" : Path.GetDirectoryName(Environment.ProcessPath) ?? "";
    }

    private static async Task<ToolConsole<T>> RunCanThrowAsync(string[] args)
    {
        System.Console.WriteLine("Initializing the console...");

        var type = typeof(T);

        var outputInterfaces = type.GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHasOutput<>))
            .ToList();

        if (!outputInterfaces.Any())
        {
            throw new ConsoleFailException("Tool must implement at least one IHasOutput<T> interface.");
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

        var consoleOptions = GetConsoleOptions(remainingArgs, configProps, out var configPropsToSet);
        var configInstances = GetConfigInstances(configPropTypes, consoleOptions);

        System.Console.WriteLine();

        foreach (var toolInstance in ToolConstructorPicker.CreateInstances<T>(inputFiles, consoleOptions.SingleOutput))
        {
            await RunToolInstanceAsync(toolInstance, configInstances, configPropsToSet, consoleOptions.OutputDir ?? rootPath);

            System.Console.WriteLine();
        }

        System.Console.WriteLine("Complete!");

        return console;
    }

    private static async Task RunToolInstanceAsync(T toolInstance, Dictionary<PropertyInfo, Config> configInstances, Dictionary<PropertyInfo, object?> configPropsToSet, string outputDir)
    {
        System.Console.WriteLine("Running tool instance...");

        foreach (var (configProp, config) in configInstances)
        {
            configProp.SetValue(toolInstance, config);

            foreach (var (prop, value) in configPropsToSet)
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

        if (toolInstance is IHasAssets toolWithAssets)
        {
            await toolWithAssets.LoadAssetsAsync();
        }

        foreach (var produceMethod in typeof(T).GetMethods().Where(m => m.Name == nameof(IHasOutput<object>.Produce)))
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

            var outputSaver = new OutputSaver(output, outputDir);
            outputSaver.Save();
        }
    }

    private static Dictionary<PropertyInfo, Config> GetConfigInstances(IList<PropertyInfo> configPropTypes, ConsoleOptions consoleOptions)
    {
        var configInstances = new Dictionary<PropertyInfo, Config>();

        if (configPropTypes.Count <= 0)
        {
            return configInstances;
        }

        Directory.CreateDirectory(Path.Combine(rootPath, "Config"));

        var customConfig = consoleOptions.CustomConfig ?? "Default";

        foreach (var configProp in configPropTypes)
        {
            var configType = configProp.PropertyType;

            var fileName = Path.Combine(rootPath, "Config", $"{customConfig}{(configPropTypes.Count > 1 ? $"_{configType.Name}" : "")}.yml");

            Config? config;

            if (File.Exists(fileName))
            {
                using var r = new StreamReader(fileName);
                config = (Config)Yml.Deserializer.Deserialize(r, configType)!;
            }
            else
            {
                config = (Config)Activator.CreateInstance(configType)!;
            }

            File.WriteAllText(fileName, Yml.Serializer.Serialize(config));

            configInstances.Add(configProp, config);
        }

        return configInstances;
    }

    private static ConsoleOptions GetConsoleOptions(IEnumerable<string> args, Dictionary<string, PropertyInfo> configProps, out Dictionary<PropertyInfo, object?> configPropsToSet)
    {
        System.Console.WriteLine();

        ConsoleOptions options;

        if (File.Exists("ConsoleOptions.yml"))
        {
            System.Console.WriteLine("Using existing ConsoleOptions.yml...");

            using var r = new StreamReader("ConsoleOptions.yml");
            options = Yml.Deserializer.Deserialize<ConsoleOptions>(r)!;
        }
        else
        {
            System.Console.WriteLine("Creating new ConsoleOptions.yml...");
            options = new ConsoleOptions();

            var games = new Dictionary<string, Func<ConsoleOptions, string?>>
            {
                { "TrackMania Forever", o => o.TrackmaniaForeverInstallationPath },
                { "ManiaPlanet", o => o.ManiaPlanetInstallationPath },
                { "Trackmania Turbo", o => o.TrackmaniaTurboInstallationPath },
                { "Trackmania 2020", o => o.Trackmania2020InstallationPath },
            };

            foreach (var (game, setting) in games)
            {
                while (true)
                {
                    System.Console.Write($"Enter your {game} installation path (leave empty if not installed or interested): ");

                    var path = System.Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(path))
                    {
                        break;
                    }

                    CopyAssets(path, game is not "TrackMania Forever");

                    break;
                }
            }
        }

        File.WriteAllText("ConsoleOptions.yml", Yml.Serializer.Serialize(options));

        configPropsToSet = new Dictionary<PropertyInfo, object?>();

        System.Console.WriteLine();
        System.Console.WriteLine("Additional arguments:");

        var argEnumerator = args.GetEnumerator();

        while (argEnumerator.MoveNext())
        {
            var arg = argEnumerator.Current;
            var argLower = arg.ToLowerInvariant();

            switch (argLower)
            {
                case "-singleoutput": // Merge will produce only one instance of Tool
                    options.SingleOutput = true;
                    System.Console.WriteLine($": {arg}");
                    continue;
                case "-config":
                    if (!argEnumerator.MoveNext())
                    {
                        throw new ConsoleFailException("Missing string value for option " + arg);
                    }

                    options.CustomConfig = argEnumerator.Current;
                    System.Console.WriteLine($": {arg} \"{options.CustomConfig}\"");
                    continue;
                case "-o":
                case "-output":
                    if (!argEnumerator.MoveNext())
                    {
                        throw new ConsoleFailException("Missing string value for option " + arg);
                    }

                    var outputDir = argEnumerator.Current;

                    if (!Directory.Exists(outputDir))
                    {
                        throw new ConsoleFailException("Output directory does not exist: " + outputDir);
                    }

                    options.OutputDir = outputDir;
                    System.Console.WriteLine($": {arg} \"{outputDir}\"");
                    continue;
            }

            if (!configProps.TryGetValue(argLower, out var confProp))
            {
                throw new ConsoleFailException("Unknown argument: " + arg);
            }

            if (confProp.PropertyType == typeof(string))
            {
                if (!argEnumerator.MoveNext())
                {
                    throw new ConsoleFailException("Missing value for config option " + arg);
                }

                var val = argEnumerator.Current;

                System.Console.WriteLine($": {arg} \"{val}\"");

                configPropsToSet.Add(confProp, val);
            }
            else
            {
                throw new ConsoleFailException($"Config option {arg} is not a string.");
            }
        }

        return options;
    }

    private static void CopyAssets(string path, bool isManiaPlanet)
    {
        NadeoIni nadeoIni;

        try
        {
            nadeoIni = NadeoIni.Parse(Path.Combine(path, "Nadeo.ini"));
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Failed to parse Nadeo.ini: {ex.Message}");
            return;
        }

        var myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        string userDataDir;

        if (!string.IsNullOrWhiteSpace(nadeoIni.UserSubDir) && string.IsNullOrWhiteSpace(nadeoIni.UserDir))
        {
            userDataDir = Path.Combine(myDocs, nadeoIni.UserSubDir);
        }
        else if (string.IsNullOrWhiteSpace(nadeoIni.UserSubDir) && !string.IsNullOrWhiteSpace(nadeoIni.UserDir))
        {
            userDataDir = nadeoIni.UserDir.Replace("{userdocs}", myDocs);
        }
        else
        {
            System.Console.WriteLine("Nadeo.ini contains invalid UserDir/UserSubDir combination.");
            return;
        }

        var assetsIdent = typeof(T).GetCustomAttribute<ToolAssetsAttribute>()?.Identifier ?? throw new Exception("Tool is missing ToolAssetsAttribute");
        var assetsDir = Path.Combine(rootPath, "Assets", "Tools", assetsIdent);

        foreach (var filePath in Directory.GetFiles(assetsDir, "*.*", SearchOption.AllDirectories))
        {
            var relativeFilePath = Path.GetRelativePath(assetsDir, filePath);

            var updatedRelativePath = typeof(T).GetMethod(nameof(IHasAssets.RemapAssetRoute))?.Invoke(null, new object[] { relativeFilePath, isManiaPlanet }) as string ?? throw new Exception("Undefined file path");
            var finalPath = Path.Combine(userDataDir, updatedRelativePath);

            System.Console.WriteLine($"Copying {relativeFilePath} to {updatedRelativePath}...");
        }

        System.Console.WriteLine("Copied!");
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

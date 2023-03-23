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

        var inputFiles = args.TakeWhile(arg => !arg.StartsWith('-')).ToArray();
        var ctorArgs = inputFiles.Select(GetNodeFromFileName).ToArray(); // Otherwise the nodes will be repeately parsed
        var remainingArgs = args.Skip(inputFiles.Length);

        var once = false;

        foreach (var arg in remainingArgs)
        {
            var argLower = arg.ToLowerInvariant();

            switch (argLower)
            {
                case "-once":
                    once = true;
                    break;
            }

            if (configProps.TryGetValue(argLower, out var confProp))
            {

            }
        }


        if (once)
        {
            var (ctor, parameterValues) = GetSuitableCtorParamsToRunOnce(ctorArgs);
            var tool = (T)ctor.Invoke(parameterValues.ToArray());
        }
        else
        {

        }

        return Task.FromResult(console);
    }

    private static Node GetNodeFromFileName(string fileName)
    {
        return GameBox.ParseNode(fileName) ?? throw new Exception();
    }

    private static (ConstructorInfo, IEnumerable<object>) GetSuitableCtorParamsToRunOnce(Node[] ctorArgs)
    {
        foreach (var constructor in typeof(T).GetConstructors())
        {
            var parameters = constructor.GetParameters();
            var nodeQueueDict = CreateNodeQueueByNodeType(ctorArgs);
            var ctorParamValues = new List<object>();

            var invalidCtor = false;

            foreach (var param in parameters)
            {
                var type = param.ParameterType;

                if (type.IsAssignableTo(typeof(IEnumerable)) && type.IsGenericType)
                {
                    var elementType = type.GetGenericArguments()[0];
                    var queueForEnumerable = nodeQueueDict[elementType];

                    var listType = typeof(List<>).MakeGenericType(elementType);

                    var nodes = (IList)Activator.CreateInstance(listType)!;

                    while (queueForEnumerable.Count > 0)
                    {
                        nodes.Add(queueForEnumerable.Dequeue());
                    }

                    ctorParamValues.Add(nodes);

                    continue;
                }

                var queue = nodeQueueDict[type];

                if (queue.Count == 0)
                {
                    invalidCtor = true;
                    break;
                }

                var node = queue.Dequeue();

                ctorParamValues.Add(node);
            }

            if (parameters.Length != ctorParamValues.Count)
            {
                continue;
            }

            foreach (var queue in nodeQueueDict)
            {
                if (queue.Value.Count > 0)
                {
                    invalidCtor = true;
                    break;
                }
            }

            if (invalidCtor)
            {
                continue;
            }

            return (constructor, ctorParamValues);
        }

        throw new Exception("No suitable constructor found");
    }

    private static Dictionary<Type, Queue<Node>> CreateNodeQueueByNodeType(Node[] ctorArgs)
    {
        var nodeQueueDict = new Dictionary<Type, Queue<Node>>();

        foreach (var ctorArg in ctorArgs)
        {
            var type = ctorArg.GetType();

            if (!nodeQueueDict.TryGetValue(type, out var queue))
            {
                queue = new Queue<Node>();
                nodeQueueDict.Add(type, queue);
            }

            queue.Enqueue(ctorArg);
        }

        return nodeQueueDict;
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

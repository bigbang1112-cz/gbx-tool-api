using GBX.NET;
using System.Text;

namespace GbxToolAPI.Console;

static class ToolConstructorPicker
{
    internal static IEnumerable<T> CreateInstances<T>(string[] files) where T : class, ITool
    {
        var inputByType = CreateInputDictionaryFromFiles(files);
        
        foreach (var ctor in typeof(T).GetConstructors())
        {
            var ctorParams = ctor.GetParameters();

            var invalidCtor = false;
            var bulkParamIndex = default(int?);
            var ctorParamValuesToServe = new object[ctorParams.Length];
            var bulkParamList = new List<object>();

            for (int i = 0; i < ctorParams.Length; i++)
            {
                var param = ctorParams[i];
                
                if (!inputByType.TryGetValue(param.ParameterType, out var inputList))
                {
                    invalidCtor = true;
                    break;
                }

                switch (inputList.Count)
                {
                    case <= 0:
                        throw new Exception("No input for parameter " + param.Name + " of type " + param.ParameterType.Name);
                    case 1:
                        ctorParamValuesToServe[i] = inputList.First();
                        break;
                    default:
                        if (bulkParamIndex is not null)
                        {
                            throw new Exception("Bulk input is supported with only one type of input.");
                        }

                        bulkParamIndex = i;
                        bulkParamList.AddRange(inputList);
                        break;
                }
            }

            if (invalidCtor)
            {
                continue;
            }

            if (bulkParamIndex is null)
            {
                yield return ctor.Invoke(ctorParamValuesToServe) as T ?? throw new Exception("Invalid constructor");
                continue;
            }

            foreach (var val in bulkParamList)
            {
                ctorParamValuesToServe[bulkParamIndex.Value] = val;
                yield return ctor.Invoke(ctorParamValuesToServe) as T ?? throw new Exception("Invalid constructor");
            }
        }
    }

    private static Dictionary<Type, ICollection<object>> CreateInputDictionaryFromFiles(string[] files)
    {
        var dict = new Dictionary<Type, ICollection<object>>();

        foreach (var typeGroup in GetFileObjectInstances(files).GroupBy(obj => obj.GetType()))
        {
            var list = new List<object>();

            foreach (var obj in typeGroup)
            {
                list.Add(obj);
            }

            dict.Add(typeGroup.Key, list);
        }

        return dict;
    }

    private static IEnumerable<object> GetFileObjectInstances(string[] files)
    {
        foreach (var file in files)
        {
            if (IsTextFile(file))
            {
                yield return new TextFile(File.ReadAllText(file));
                continue;
            }

            var node = GameBox.ParseNode(file);

            yield return node is null ? new BinFile(File.ReadAllBytes(file)) : node;
        }
    }

    public static bool IsTextFile(string filePath)
    {
        try
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var r = new StreamReader(fs, Encoding.UTF8, true, 1024, true);
            
            while (!r.EndOfStream)
            {
                int charValue = r.Read();
                if (charValue == 0)
                {
                    // file has null byte, considered binary
                    return false;
                }
            }
            
            // file doesn't contain null bytes or invalid UTF-8 sequences, considered text
            return true;
        }
        catch (DecoderFallbackException)
        {
            // invalid UTF-8 sequence, considered binary
            return false;
        }
    }
}
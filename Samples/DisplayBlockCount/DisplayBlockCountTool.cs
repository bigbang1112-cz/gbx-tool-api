using GBX.NET.Engines.Game;
using GbxToolAPI;
using TmEssentials;

namespace DisplayBlockCount;

[ToolName("Display Block Count")]
[ToolDescription("Display Block Count is a GBX.NET web tool.")]
[ToolAuthors("BigBang1112")]
public class DisplayBlockCountTool : ITool, IHasOutput<NodeFile<CGameCtnChallenge>>, IConfigurable<DisplayBlockCountConfig>
{
    private readonly CGameCtnChallenge map;

    public DisplayBlockCountConfig Config { get; set; } = new();

    public DisplayBlockCountTool(CGameCtnChallenge map)
    {
        this.map = map;
    }

    public NodeFile<CGameCtnChallenge> Produce()
    {
        Console.WriteLine(map.Blocks?.Count);

        var pureFileName = $"{TextFormatter.Deformat(map.MapName)}.Map.Gbx";
        var validFileName = string.Join("_", pureFileName.Split(Path.GetInvalidFileNameChars()));

        return new(map, $"Maps/DisplayBlockCount/{validFileName}", IsManiaPlanet: true);
    }
}
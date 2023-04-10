using GBX.NET.Engines.Game;

namespace GbxToolAPI;

public static class GameVersion
{
    public static bool IsManiaPlanet(CGameCtnReplayRecord replay)
    {
        return replay.Chunks.Any(chunk => chunk.Id > 0x03093015 && chunk.Id <= 0x03093FFF);
    }
    
    public static bool IsManiaPlanet(CGameCtnGhost ghost)
    {
        return ghost.Chunks.Any(chunk => chunk.Id > 0x03092019 && chunk.Id <= 0x03092FFF);
    }

    public static bool IsManiaPlanet(CGameCtnChallenge map)
    {
        return map.Chunks.Any(chunk => chunk.Id > 0x0304302A && chunk.Id <= 0x03043FFF);
    }
}

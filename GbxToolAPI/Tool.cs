namespace GbxToolAPI;

public abstract class Tool
{
    // has to have parameterless constructor

    static Tool()
    {
        GBX.NET.Lzo.SetLzo(typeof(GBX.NET.LZO.MiniLZO));
    }
}

namespace GbxToolAPI;

public abstract class Tool<TConfig> : Config
{
    // has to have parameterless constructor

    public abstract TConfig Config { get; set; }

    static Tool()
    {
        GBX.NET.Lzo.SetLzo(typeof(GBX.NET.LZO.MiniLZO));
    }
}

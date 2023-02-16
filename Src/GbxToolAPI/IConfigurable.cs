namespace GbxToolAPI;

public interface IConfigurable<TConfig> where TConfig : Config
{
    public abstract TConfig Config { get; set; }
}

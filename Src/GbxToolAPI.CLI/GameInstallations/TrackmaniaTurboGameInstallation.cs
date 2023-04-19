namespace GbxToolAPI.CLI.GameInstallations;

internal class TrackmaniaTurboGameInstallation : GameInstallation
{
    public override string Name => Constants.TrackmaniaTurbo;
    public override string ExeName => "TrackmaniaTurbo";

    public override string[] SuggestedInstallationPaths { get; } = new[]
    {
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Ubisoft", "Ubisoft Game Launcher", "games", "TrackmaniaTurbo"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Ubisoft", "Ubisoft Game Launcher", "games", "TrackmaniaTurbo")
    };

    public override string? GetPathFromOptions(ConsoleOptions options)
    {
        return options.TrackmaniaTurboInstallationPath;
    }

    public override void SetPathFromOptions(ConsoleOptions options, string? path)
    {
        options.TrackmaniaTurboInstallationPath = path;
    }
}

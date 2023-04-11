using System.Transactions;

namespace GbxToolAPI.Console;

public class NadeoIni
{
    public string? UserSubDir { get; init; }
    public string? UserDir { get; init; }

    public static NadeoIni Parse(string nadeoIniFilePath)
    {
        var anyOfDirs = false;
        var userSubDir = "TmForever";
        var userDir = default(string);

        foreach (var line in File.ReadLines(nadeoIniFilePath))
        {
            if (line.Length == 0 || line[0] is '#' or ';' or '[')
            {
                continue;
            }

            if (line.StartsWith("UserSubDir="))
            {
                userSubDir = line[11..];
                anyOfDirs = true;
            }

            if (line.StartsWith("UserDir="))
            {
                userDir = line[8..];
                anyOfDirs = true;
            }
        }

        if (!anyOfDirs)
        {
            throw new Exception("No UserSubDir or UserDir found.");
        }

        return new NadeoIni
        {
            UserSubDir = userSubDir,
            UserDir = userDir
        };
    }
}
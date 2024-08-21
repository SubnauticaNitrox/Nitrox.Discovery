using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Nitrox.Discovery.InstallationFinders.Core;

namespace Nitrox.Discovery.MSBuild;

/// <summary>
///     Discovers the path to a game from its name as installed on the current machine.
/// </summary>
public class DiscoverGame : Task
{
    [Required]
    public string GameName { get; set; }

    [Required]
    public string IntermediateOutputPath { get; set; }

    [Output]
    public string GamePath { get; set; }

    public override bool Execute()
    {
        if (string.IsNullOrWhiteSpace(GameName))
        {
            Log.LogWarning($@"Property ""{nameof(GameName)}"" is required");
            return false;
        }

        // Read from cache
        if (!string.IsNullOrEmpty(IntermediateOutputPath))
        {
            Directory.CreateDirectory(Path.Combine(IntermediateOutputPath, "nitroxdiscovery"));
            GamePath = ReadGamePath();
        }
        if (Directory.Exists(GamePath))
        {
            return true;
        }

        // Refresh cache
        GameFinderResult finderResult = GameInstallationFinder.Instance.FindGame(new GameInfo
        {
            Name = GameName
        }).FirstOrDefault(r => r.IsOk);
        GamePath = finderResult?.Path ?? "";
        StoreGamePath(GamePath);

        return true;
    }

    private void StoreGamePath(string gamePath)
    {
        if (string.IsNullOrEmpty(IntermediateOutputPath))
        {
            return;
        }

        string cacheFile = Path.Combine(IntermediateOutputPath, "nitroxdiscovery", Path.ChangeExtension(GameName, "cache"));
        File.WriteAllText(cacheFile, gamePath);
    }

    private string ReadGamePath()
    {
        if (string.IsNullOrEmpty(IntermediateOutputPath))
        {
            return "";
        }

        string cacheFile = Path.Combine(IntermediateOutputPath, "nitroxdiscovery", Path.ChangeExtension(GameName, "cache"));
        try
        {
            return File.ReadAllText(cacheFile).Trim();
        }
        catch (FileNotFoundException)
        {
            return "";
        }
    }
}

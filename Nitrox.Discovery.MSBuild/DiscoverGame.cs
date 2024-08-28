using System;
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

    /// <summary>
    ///     Name of the game executable without the exe extension.
    /// </summary>
    public string ExeName { get; set; } = "";

    /// <summary>
    ///     Relative depth to search within the game root for the game executable.
    /// </summary>
    public int ExeSearchDepth { get; set; } = 0;

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
        FinderResult finderResult = GameInstallationFinder.Instance.FindGame(new GameInfo
        {
            Name = GameName,
            ExeName = ExeName ?? "",
            ExeSearchDepth = Math.Max(0, ExeSearchDepth),
        }).FirstOrDefault(r => string.IsNullOrWhiteSpace(r.ErrorMessage) && !string.IsNullOrWhiteSpace(r.Path));
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

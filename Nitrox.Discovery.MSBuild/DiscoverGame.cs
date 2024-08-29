using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Nitrox.Discovery.Extensions;
using Nitrox.Discovery.InstallationFinders.Core;
using Nitrox.Discovery.Models;

namespace Nitrox.Discovery.MSBuild;

/// <summary>
///     Discovers the path to a game from its name as installed on the current machine.
/// </summary>
public class DiscoverGame : Task
{
    private const string DiscoverGameCacheFolderName = "Nitrox Discovery MSBuild";

    [Required]
    public string GameName { get; set; }

    /// <summary>
    ///     Name of the game executable, with or without extension.
    /// </summary>
    public string ExeName { get; set; } = "";

    /// <summary>
    ///     Relative depth to search within the game root for the game executable.
    /// </summary>
    public int ExeSearchDepth { get; set; } = 0;

    /// <summary>
    ///     Specifies the libraries to include in the search. The search will start in the same order as given.
    ///     Leave empty to search all libraries. See <see cref="GameLibraries" /> for valid library names.
    /// </summary>
    /// <remarks>
    ///     End with "All" to include all libraries in the search, but start the search with the preceding library names.
    ///     For example: <code>Steam;Epic;All</code>
    /// </remarks>
    public ITaskItem[] IncludeLibraries { get; set; } = [];

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
        IntermediateOutputPath = string.IsNullOrWhiteSpace(IntermediateOutputPath) ? Path.Combine(".", "obj") : IntermediateOutputPath;
        if (!Directory.Exists(IntermediateOutputPath))
        {
            IntermediateOutputPath = "";
        }

        // Read from cache
        if (!string.IsNullOrEmpty(IntermediateOutputPath))
        {
            Directory.CreateDirectory(Path.Combine(IntermediateOutputPath, DiscoverGameCacheFolderName));
            GamePath = ReadGamePath();
        }
        if (Directory.Exists(GamePath))
        {
            return true;
        }

        // Refresh cache
        IEnumerable<GameLibraries> libraries;
        if (IncludeLibraries is [] or null)
        {
            libraries = GameLibraries.ALL.GetUniqueNonCombinatoryFlags();
        }
        else
        {
            libraries = IncludeLibraries
                .Select((library, index) =>
                {
                    if (!Enum.TryParse(library.ItemSpec, true, out GameLibraries gameLibrary))
                    {
                        throw new ArgumentOutOfRangeException($@"Unknown game library ""{library.ItemSpec}""");
                    }

                    return new { Index = index, GameLibrary = gameLibrary.GetUniqueNonCombinatoryFlags() };
                }).OrderBy(l => l.Index)
                .SelectMany(l => l.GameLibrary)
                .Distinct();
        }
        FinderResult finderResult = GameInstallationFinder.Instance.FindGame(new()
        {
            Name = GameName,
            ExeName = ExeName ?? "",
            ExeSearchDepth = Math.Max(0, ExeSearchDepth)
        }, libraries).FirstOrDefault(r => string.IsNullOrWhiteSpace(r.ErrorMessage) && !string.IsNullOrWhiteSpace(r.Path));
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

        string cacheFile = Path.Combine(IntermediateOutputPath, DiscoverGameCacheFolderName, Path.ChangeExtension(GameName, "cache"));
        File.WriteAllText(cacheFile, gamePath);
    }

    private string ReadGamePath()
    {
        if (string.IsNullOrEmpty(IntermediateOutputPath))
        {
            return "";
        }

        string cacheFile = Path.Combine(IntermediateOutputPath, DiscoverGameCacheFolderName, Path.ChangeExtension(GameName, "cache"));
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
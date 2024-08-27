using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Nitrox.Discovery.InstallationFinders.Core;

namespace Nitrox.Discovery.InstallationFinders;

/// <summary>
/// Trying to find the path in the Epic Games installation records.
/// </summary>
public sealed class EpicGamesFinder : IGameFinder
{
    private static readonly Regex installLocationRegex = new("\"InstallLocation\"[^\"]*\"(.*)\"");

    public IEnumerable<FinderResult> FindGame(GameInfo gameInfo)
    {
        string commonAppFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        string epicGamesManifestsDir = Path.Combine(commonAppFolder, "Epic", "EpicGamesLauncher", "Data", "Manifests");

        if (!Directory.Exists(epicGamesManifestsDir))
        {
            yield return FinderResult.Error("Epic games manifest directory does not exist. Verify that Epic Games Store has been installed");
        }

        string[] files = Directory.GetFiles(epicGamesManifestsDir, "*.item");
        foreach (string file in files)
        {
            string fileText = File.ReadAllText(file);
            Match match = installLocationRegex.Match(fileText);

            if (match.Success && match.Value.Contains(gameInfo.Name))
            {
                yield return Path.GetFullPath(match.Groups[1].Value);
            }
        }

        yield return FinderResult.Error("Could not find game installation directory from Epic Games installation records. Verify that game has been installed with Epic Games Store");
    }
}

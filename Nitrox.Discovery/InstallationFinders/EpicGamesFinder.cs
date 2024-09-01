using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Nitrox.Discovery.InstallationFinders.Core;

namespace Nitrox.Discovery.InstallationFinders;

/// <summary>
///     Trying to find the path in the Epic Games installation records.
/// </summary>
public sealed class EpicGamesFinder : IGameFinder
{
    private static readonly Regex itemFilePropertyLineRegex = new(@"""([^""]*)"":\s*""([^""]*)""");

    public IEnumerable<FinderResult> FindGame(FindGameInfo input)
    {
        string commonAppFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        string epicGamesManifestsDir = Path.Combine(commonAppFolder, "Epic", "EpicGamesLauncher", "Data", "Manifests");

        if (!Directory.Exists(epicGamesManifestsDir))
        {
            yield return FinderResult.Error("Epic games manifest directory does not exist. Verify that Epic Games Store has been installed");
        }
        string[] files;
        try
        {
            files = Directory.GetFiles(epicGamesManifestsDir, "*.item");
        }
        catch (IOException)
        {
            yield break;
        }

        foreach (string file in files)
        {
            (string displayName, string installLocation) = GetNameAndInstallFromItemFile(file);
            if (input.IsSimilarGameName(displayName) && !string.IsNullOrWhiteSpace(installLocation))
            {
                yield return Path.GetFullPath(installLocation);
            }
        }
    }

    private (string displayName, string installLocation) GetNameAndInstallFromItemFile(string itemFile)
    {
        string displayName = "";
        string installLocation = "";
        try
        {
            using StreamReader file = new(itemFile);

            while (file.ReadLine() is { } line)
            {
                line = Regex.Unescape(line);
                Match regMatch = itemFilePropertyLineRegex.Match(line);
                string key = regMatch.Groups[1].Value.ToLowerInvariant();
                string value = regMatch.Groups[2].Value;

                switch (key)
                {
                    case "displayname":
                        displayName = value;
                        break;
                    case "installlocation":
                        installLocation = value;
                        break;
                }

                if (displayName != "" && installLocation != "")
                {
                    break;
                }
            }
        }
        catch (IOException)
        {
            // ignored
        }
        return (displayName, installLocation);
    }
}
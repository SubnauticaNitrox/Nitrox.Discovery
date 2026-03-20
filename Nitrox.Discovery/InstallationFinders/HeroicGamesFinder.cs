using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using Nitrox.Discovery.InstallationFinders.Core;

namespace Nitrox.Discovery.InstallationFinders;

/// <summary>
/// Trying to find the path in the Heroic-Games-Launcher installation records.
/// </summary>
public sealed class HeroicGamesFinder : IGameFinder
{
    public IEnumerable<GameFinderResult> FindGame(FindGameInfo gameInfo)
    {
        string standardConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "heroic");
        if (TryGetKnownGameByConfig(gameInfo, standardConfigPath, out string result))
        {
            yield return result;
        }

        if (IsOSPlatform(OSPlatform.Linux))
        {
            string flatpakConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".var", "app", "com.heroicgameslauncher.hgl", "config", "heroic");
            if (TryGetKnownGameByConfig(gameInfo, flatpakConfigPath, out result))
            {
                yield return result;
            }
        }
    }

    private static bool TryGetKnownGameByConfig(FindGameInfo gameInfo, string heroicRootConfigPath, out string installPath)
    {
        installPath = "";
        string installConfigFilePath = Path.Combine(heroicRootConfigPath, "legendaryConfig", "legendary", "installed.json");
        if (!File.Exists(installConfigFilePath))
        {
            return false;
        }
        string configJson = File.ReadAllText(installConfigFilePath);
        return TryGetGameInstallFromJson(configJson, gameInfo, out installPath);
    }

    private static bool TryGetGameInstallFromJson(string installedJson, FindGameInfo gameInfo, out string installPath)
    {
        installPath = "";
        JsonNode? jsonRoot = JsonNode.Parse(installedJson);
        if (jsonRoot == null)
        {
            return false;
        }

        foreach (JsonNode? node in jsonRoot.Children())
        {
            installPath = node["install_path"]?.GetValue<string>() ?? "";
            if (string.IsNullOrWhiteSpace(installPath))
            {
                continue;
            }
            if (Path.GetFileName(installPath) != gameInfo.NormalizedGameName)
            {
                continue;
            }
            if (!string.IsNullOrWhiteSpace(gameInfo.ExeName) && node["executable"]?.GetValue<string>() != gameInfo.ExeName)
            {
                continue;
            }

            return true;
        }

        return false;
    }
}
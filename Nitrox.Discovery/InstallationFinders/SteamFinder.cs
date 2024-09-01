using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Nitrox.Discovery.InstallationFinders.Core;
using static Nitrox.Discovery.InstallationFinders.Core.FinderResult;

namespace Nitrox.Discovery.InstallationFinders;

/// <summary>
///     Tries to find the path in the Steam installation directory by the name of the game.
///     By default, each game will have a corresponding appmanifest_{appid}.acf file in the steamapps folder.
///     Except for some games that are installed on a different disk drive, in those case 'libraryfolders.vdf' will give us the real location of the folder containing the acf files.
/// </summary>
public sealed class SteamFinder : IGameFinder
{
    private static readonly Regex xcfPropertyLineRegex = new(@"""([^""]*)""\s*""([^""]*)""");
    private static readonly string[] acfGameNameAndIdKeys = ["appid", "name"];
    private static readonly char[] acfLineTrimCharacters = [' ', '\t'];

    public IEnumerable<FinderResult> FindGame(FindGameInfo input)
    {
        string steamPath = GetSteamPath();
        if (string.IsNullOrEmpty(steamPath))
        {
            yield return Error("Steam is not installed");
        }

        string appsPath = Path.Combine(steamPath, "steamapps");
        int steamAppId = GetSteamAppIdFromAcfFileMatchingGameName(appsPath, input.GameName);

        string path;
        if (File.Exists(Path.Combine(appsPath, $"appmanifest_{steamAppId}.acf")))
        {
            path = Path.Combine(appsPath, "common", input.GameName);
        }
        else
        {
            path = SearchAllInstallations(Path.Combine(appsPath, "libraryfolders.vdf"), input.GameName);
            if (string.IsNullOrWhiteSpace(path))
            {
                yield break;
            }
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            path = Path.Combine(path, $"{input.GameName}.app", "Contents");
        }
        yield return path;
    }

    private static int GetSteamAppIdFromAcfFileMatchingGameName(string rootDirectory, string gameName)
    {
        if (!Directory.Exists(rootDirectory))
        {
            return -1;
        }
        string[] acfFiles;
        try
        {
            acfFiles = Directory.EnumerateFiles(rootDirectory, "appmanifest_*.acf", SearchOption.TopDirectoryOnly).ToArray();
        }
        catch (IOException)
        {
            return -1;
        }

        foreach (string acfFile in acfFiles)
        {
            Dictionary<string,string> props = ExtractPropertiesFromXcfFile(acfFile, acfGameNameAndIdKeys);
            if (!props.TryGetValue("name", out string extractedGameName) || !extractedGameName.Equals(gameName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            if (props.TryGetValue("appid", out string extractedAppId) && int.TryParse(extractedAppId, out int appId))
            {
                return appId;
            }
        }

        return -1;
    }

    private static string GetSteamPath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string steamPath = RegistryEx.Read<string>(@"Software\Valve\Steam\SteamPath");

            if (string.IsNullOrWhiteSpace(steamPath))
            {
                steamPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    "Steam"
                );
            }

            return Directory.Exists(steamPath) ? steamPath : null;
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (string.IsNullOrWhiteSpace(homePath))
            {
                homePath = Environment.GetEnvironmentVariable("HOME");
            }

            if (!Directory.Exists(homePath))
            {
                return null;
            }

            string[] commonSteamPath =
            [
                // Default install location
                // https://github.com/ValveSoftware/steam-for-linux
                Path.Combine(homePath, ".local", "share", "Steam"),
                // Those symlinks are often use as a backward-compatibility (Debian, Ubuntu, Fedora, ArchLinux)
                // https://wiki.archlinux.org/title/steam, https://askubuntu.com/questions/227502/where-are-steam-games-installed
                Path.Combine(homePath, ".steam", "steam"),
                Path.Combine(homePath, ".steam", "root"),
                // Flatpack install
                // https://github.com/flathub/com.valvesoftware.Steam/wiki, https://flathub.org/apps/com.valvesoftware.Steam
                Path.Combine(homePath, ".var", "app", "com.valvesoftware.Steam", ".local", "share", "Steam"),
                Path.Combine(homePath, ".var", "app", "com.valvesoftware.Steam", ".steam", "steam")
            ];

            foreach (string path in commonSteamPath)
            {
                if (Directory.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (string.IsNullOrWhiteSpace(homePath))
            {
                homePath = Environment.GetEnvironmentVariable("HOME");
            }
            if (!Directory.Exists(homePath))
            {
                return null;
            }

            // Steam should always be here
            string steamPath = Path.Combine(homePath, "Library", "Application Support", "Steam");
            if (Directory.Exists(steamPath))
            {
                return steamPath;
            }
            return null;
        }

        return null;
    }

    /// <summary>
    ///     Finds game install directory by iterating through all the steam game libraries configured, matching the given appid.
    /// </summary>
    private static string SearchAllInstallations(string libraryFolders, string gameName)
    {
        if (!File.Exists(libraryFolders))
        {
            return null;
        }

        using StreamReader file = new(libraryFolders);
        char[] trimChars = [' ', '\t'];

        while (file.ReadLine() is { } line)
        {
            line = Regex.Unescape(line.Trim(trimChars));
            Match regMatch = xcfPropertyLineRegex.Match(line);
            string key = regMatch.Groups[1].Value;

            // New format (about 2021-07-16) uses "path" key instead of steam-library-index as key. If either, it could be steam game path.
            if (!key.Equals("path", StringComparison.OrdinalIgnoreCase) && !int.TryParse(key, out _))
            {
                continue;
            }

            string value = regMatch.Groups[2].Value;

            int appId = GetSteamAppIdFromAcfFileMatchingGameName(Path.Combine(value, "steamapps"), gameName);
            if (appId > -1)
            {
                return Path.Combine(value, "steamapps", "common", gameName);
            }
        }

        return null;
    }

    /// <summary>
    ///     Simplified acf/vdf file parser which extracts properties found. Only the first property key and their value is saved.
    /// </summary>
    private static Dictionary<string, string> ExtractPropertiesFromXcfFile(string xcfFile, params string[] keysToFind)
    {
        try
        {
            using StreamReader file = new(xcfFile);

            int keysToFindCount = keysToFind.Length;
            Dictionary<string, string> result = [];
            while (file.ReadLine() is { } line)
            {
                line = Regex.Unescape(line.Trim(acfLineTrimCharacters));
                Match regMatch = xcfPropertyLineRegex.Match(line);
                string key = regMatch.Groups[1].Value;
                string value = regMatch.Groups[2].Value;

                if (keysToFind.Length > 0)
                {
                    if (!keysToFind.Contains(key, StringComparer.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }
                    keysToFindCount--;
                }

                // Add key & value to result.
                key = key.ToLowerInvariant();
                if (!result.ContainsKey(key))
                {
                    result[key] = value;
                }

                if (keysToFind.Length > 0)
                {
                    if (keysToFindCount < 1)
                    {
                        break;
                    }
                }
            }
            return result;
        }
        catch (IOException)
        {
            return [];
        }
    }
}
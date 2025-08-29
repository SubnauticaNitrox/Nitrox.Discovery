using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Nitrox.Discovery.InstallationFinders;
using Nitrox.Discovery.InstallationFinders.Core;
using Nitrox.Discovery.Models;

namespace Nitrox.Discovery;

/// <summary>
///     Main game installation finder that will use all available methods of detection to find the game installation directory
/// </summary>
public sealed class GameInstallationFinder
{
    private static readonly Lazy<GameInstallationFinder> instance = new(() => new(new()
    {
        { GameLibraries.STEAM, new SteamFinder() },
        { GameLibraries.EPIC, new EpicGamesFinder() },
        { GameLibraries.DISCORD, new DiscordFinder() },
        { GameLibraries.MICROSOFT, new MicrosoftFinder() },
        { GameLibraries.GOG, new GogFinder() }
    }));

    private readonly Dictionary<GameLibraries, IGameFinder> finders;

    public GameInstallationFinder(Dictionary<GameLibraries, IGameFinder> finders)
    {
        this.finders = finders;
    }

    public static GameInstallationFinder Instance => instance.Value;

    /// <summary>
    ///     Searches for the game install directory given its <see cref="FindGameInfo" />.
    /// </summary>
    /// <param name="input">Input data containing game name to find.</param>
    /// <param name="gameLibraries">Known game libraries to search through</param>
    /// <returns>Positive and negative results from the search</returns>
    public IEnumerable<FinderResult> FindGame(FindGameInfo input, GameLibraries gameLibraries = GameLibraries.ALL)
    {
        return FindGame(input, gameLibraries.GetUniqueNonCombinatoryFlags());
    }

    /// <inheritdoc cref="FindGame(Nitrox.Discovery.FindGameInfo,Nitrox.Discovery.Models.GameLibraries)" />
    public IEnumerable<FinderResult> FindGame(FindGameInfo input, IEnumerable<GameLibraries> gameLibraries)
    {
        Debug.Assert(input is not null);
        if (input is null || gameLibraries is null)
        {
            yield break;
        }

        foreach (GameLibraries library in gameLibraries)
        {
            if (!finders.TryGetValue(library, out IGameFinder finder))
            {
                continue;
            }

            FinderResult result = null;
            foreach (FinderResult item in finder.FindGame(input))
            {
                if (item is null)
                {
                    continue;
                }
                if (item.ErrorMessage is not null)
                {
                    result = item;
                    break;
                }
                if (!PathHasExecutable(item.Path, input.ExeName, input.ExeSearchDepth))
                {
                    continue;
                }

                result = item with { Origin = library, Path = PrettifyPath(item.Path!) };
                break;
            }

            yield return result ?? new()
            {
                FinderName = finder.GetType().Name,
                Origin = library,
                ErrorMessage = $"It appears you don't have {input.GameName} installed"
            };
        }
    }

    private static bool PathHasExecutable(string directory, string executableNameOrEmpty, int maxDepth = 0)
    {
        if (maxDepth < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxDepth), maxDepth, $"The {nameof(maxDepth)} must be a positive number.");
        }
        if (executableNameOrEmpty is null)
        {
            throw new ArgumentNullException(nameof(executableNameOrEmpty));
        }
        if (!Directory.Exists(directory))
        {
            return false;
        }

        string extension = Path.GetExtension(executableNameOrEmpty).ToLowerInvariant();
        try
        {
            executableNameOrEmpty = Path.GetFileNameWithoutExtension(executableNameOrEmpty);
            foreach (string entry in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
            {
                if (entry.GetPathDepth(directory!) - 1 > maxDepth)
                {
                    // "EnumerateFiles" will do breath-first-search so we can break as soon as we hit depth limit.
                    break;
                }
                if ((extension != "" && !Path.GetExtension(entry).Equals(extension, StringComparison.OrdinalIgnoreCase)) || !entry.IsExecutableFile())
                {
                    continue;
                }
                if (executableNameOrEmpty != "" && !executableNameOrEmpty.Equals(Path.GetFileNameWithoutExtension(entry), StringComparison.Ordinal))
                {
                    continue;
                }

                return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private static string PrettifyPath(string path) => Path.GetFullPath(path);
}
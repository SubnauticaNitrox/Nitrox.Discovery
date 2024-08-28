using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Nitrox.Discovery.InstallationFinders;
using Nitrox.Discovery.InstallationFinders.Core;
using Nitrox.Discovery.Models;

namespace Nitrox.Discovery;

/// <summary>
/// Main game installation finder that will use all available methods of detection to find the game installation directory
/// </summary>
public sealed class GameInstallationFinder
{
    private static readonly Lazy<GameInstallationFinder> instance = new(() => new GameInstallationFinder(new()
    {
        { GameLibraries.STEAM, new SteamFinder() },
        { GameLibraries.EPIC, new EpicGamesFinder() },
        { GameLibraries.DISCORD, new DiscordFinder() },
        { GameLibraries.MICROSOFT, new MicrosoftFinder() },
        { GameLibraries.GOG, new GogFinder() },
    }));

    public static GameInstallationFinder Instance => instance.Value;

    private readonly Dictionary<GameLibraries, IGameFinder> finders;

    public GameInstallationFinder(Dictionary<GameLibraries, IGameFinder> finders)
    {
        this.finders = finders;
    }

    /// <summary>
    ///     Searches for the game install directory given its <see cref="FindGameInfo"/>.
    /// </summary>
    /// <param name="gameInfo">Info object of a game.</param>
    /// <param name="gameLibraries">Known game libraries to search through</param>
    /// <returns>Positive and negative results from the search</returns>
    public IEnumerable<FinderResult> FindGame(FindGameInfo gameInfo, GameLibraries gameLibraries = GameLibraries.ALL)
    {
        Debug.Assert(gameInfo is not null);
        if (gameInfo is null || !gameLibraries.IsDefined())
        {
            return [];
        }

        return FindGame(gameInfo, gameLibraries.GetUniqueNonCombinatoryFlags());
    }

    /// <inheritdoc cref="FindGame(Nitrox.Discovery.FindGameInfo,Nitrox.Discovery.Models.GameLibraries)"/>
    public IEnumerable<FinderResult> FindGame(FindGameInfo gameInfo, IEnumerable<GameLibraries> gameLibraries)
    {
        Debug.Assert(gameInfo is not null);
        if (gameInfo is null || gameLibraries is null)
        {
            yield break;
        }

        foreach (GameLibraries wantedFinder in gameLibraries)
        {
            if (!finders.TryGetValue(wantedFinder, out IGameFinder finder))
            {
                continue;
            }

            bool finderHasResult = false;
            foreach (FinderResult item in finder.FindGame(gameInfo))
            {
                if (item is null)
                {
                    continue;
                }
                FinderResult result = item;
                if (result.ErrorMessage is not null)
                {
                    yield return result;
                    finderHasResult = true;
                    break;
                }
                if (!PathHasExecutable(result.Path, gameInfo.ExeName, gameInfo.ExeSearchDepth))
                {
                    continue;
                }

                finderHasResult = true;
                yield return result with { Origin = wantedFinder, Path = PrettifyPath(result.Path!) };
                break;
            }

            if (!finderHasResult)
            {
                yield return new FinderResult
                {
                    FinderName = finder.GetType().Name,
                    Origin = wantedFinder,
                    ErrorMessage = $"It appears you don't have {gameInfo.Name} installed"
                };
            }
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
            foreach (string entry in Directory.EnumerateFileSystemEntries(directory, "*", SearchOption.AllDirectories))
            {
                if (entry.GetPathDepth(directory!) - 1 > maxDepth)
                {
                    // "EnumerateFileSystemEntries" will do breath-first-search so we can break as soon as we hit depth limit.
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

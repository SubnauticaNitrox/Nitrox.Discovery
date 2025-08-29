using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Nitrox.Discovery.InstallationFinders.Core;

namespace Nitrox.Discovery.InstallationFinders;

/// <summary>
///     Finds games installed using GOG Galaxy.
/// </summary>
public sealed class GogFinder : IGameFinder
{
    private const string GogGamesInRegistry = @"Computer\HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\GOG.com\Games";

    public IEnumerable<FinderResult> FindGame(FindGameInfo input)
    {
        // TODO: Add support for non-windows.
        if (!IsOSPlatform(OSPlatform.Windows))
        {
            yield break;
        }

        foreach (string gameId in RegistryEx.GetSubKeyNames(GogGamesInRegistry))
        {
            string gamePath = RegistryEx.Read<string>(Path.Combine(GogGamesInRegistry, gameId, "path"));
            if (input.IsSimilarGameName(Path.GetFileName(gamePath)))
            {
                yield return gamePath;
            }
            else if (input.IsSimilarGameName(RegistryEx.Read<string>(Path.Combine(GogGamesInRegistry, gameId, "gameName"))))
            {
                yield return gamePath;
            }
        }
    }
}
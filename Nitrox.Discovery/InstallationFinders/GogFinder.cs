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
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            yield break;
        }

        foreach (string gameId in RegistryEx.GetSubKeyNames(GogGamesInRegistry))
        {
            string gamePath = RegistryEx.Read<string>(Path.Combine(GogGamesInRegistry, gameId, "path"));
            if (Path.GetFileName(gamePath) == input.GameName)
            {
                yield return gamePath;
            }
            else if (RegistryEx.Read<string>(Path.Combine(GogGamesInRegistry, gameId, "gameName")) == input.GameName)
            {
                yield return gamePath;
            }
        }
    }
}
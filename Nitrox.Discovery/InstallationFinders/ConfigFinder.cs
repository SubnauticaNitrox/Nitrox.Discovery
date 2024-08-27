using System.Collections.Generic;
using Nitrox.Discovery.InstallationFinders.Core;

namespace Nitrox.Discovery.InstallationFinders;

/// <summary>
///     Tries to read a local config value that contains the installation directory.
/// </summary>
internal sealed class ConfigFinder : IGameFinder
{
    public IEnumerable<FinderResult> FindGame(GameInfo gameInfo)
    {
        // TODO: Read from cached location
        yield break;
    }
}

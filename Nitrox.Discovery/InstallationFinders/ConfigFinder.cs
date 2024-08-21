using Nitrox.Discovery.InstallationFinders.Core;
using static Nitrox.Discovery.InstallationFinders.Core.GameFinderResult;

namespace Nitrox.Discovery.InstallationFinders;

/// <summary>
///     Tries to read a local config value that contains the installation directory.
/// </summary>
internal sealed class ConfigFinder : IGameFinder
{
    public GameFinderResult FindGame(GameInfo gameInfo)
    {
        // TODO: Read from cached location
        return NotFound();
    }
}

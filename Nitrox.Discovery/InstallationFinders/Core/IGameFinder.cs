using System.Collections.Generic;

namespace Nitrox.Discovery.InstallationFinders.Core;

public interface IGameFinder
{
    /// <summary>
    ///     Searches for game installation directory.
    /// </summary>
    /// <param name="gameInfo">Game to search for.</param>
    /// <returns>Nullable game installation</returns>
    IEnumerable<FinderResult> FindGame(FindGameInfo gameInfo);
}
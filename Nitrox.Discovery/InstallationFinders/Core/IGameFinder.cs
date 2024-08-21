namespace Nitrox.Discovery.InstallationFinders.Core;

internal interface IGameFinder
{
    /// <summary>
    ///     Searches for game installation directory.
    /// </summary>
    /// <param name="gameInfo">Game to search for.</param>
    /// <returns>Nullable game installation</returns>
    GameFinderResult FindGame(GameInfo gameInfo);
}

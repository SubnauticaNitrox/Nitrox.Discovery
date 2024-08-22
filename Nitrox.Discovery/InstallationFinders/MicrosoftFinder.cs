using System.IO;
using Nitrox.Discovery.InstallationFinders.Core;
using static Nitrox.Discovery.InstallationFinders.Core.GameFinderResult;

namespace Nitrox.Discovery.InstallationFinders;

/// <summary>
/// MS Store games are stored under <c>C:\XboxGames\[GAME]\Content\</c> by default.
/// It's likely we could read the choosen path from <c>C:\Program Files\WindowsApps</c> but we're unable to read store settings from those folders.
/// </summary>
public sealed class MicrosoftFinder : IGameFinder
{
    public GameFinderResult FindGame(GameInfo gameInfo)
    {
        string path = Path.Combine("C:", "XboxGames", gameInfo.Name, "Content");
        if (!path.IsDirectoryWithTopLevelExecutable())
        {
            return Error($"Game installation directory '{path}' is invalid. Please enter the path to the '{gameInfo.Name}' installation");
        }

        return Ok(path);
    }
}

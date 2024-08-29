using System.Collections.Generic;
using System.IO;
using Nitrox.Discovery.InstallationFinders.Core;

namespace Nitrox.Discovery.InstallationFinders;

/// <summary>
///     MS Store games are stored under <c>C:\XboxGames\[GAME]\Content\</c> by default.
///     It's likely we could read the choosen path from <c>C:\Program Files\WindowsApps</c> but we're unable to read store settings from those folders.
/// </summary>
public sealed class MicrosoftFinder : IGameFinder
{
    public IEnumerable<FinderResult> FindGame(FindGameInfo gameInfo)
    {
        string[] logicalDrives = [];
        try
        {
            // TODO: Read the user defined path from MS Store app
            logicalDrives = Directory.GetLogicalDrives();
        }
        catch
        {
            // ignored
        }

        foreach (string logicalDrive in logicalDrives)
        {
            yield return Path.Combine(logicalDrive, "XboxGames", gameInfo.Name, "Content");
        }
        yield return Path.Combine("C:\\", "XboxGames", gameInfo.Name, "Content");
    }
}
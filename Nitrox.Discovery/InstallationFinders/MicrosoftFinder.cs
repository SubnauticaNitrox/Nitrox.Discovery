using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Nitrox.Discovery.InstallationFinders.Core;

namespace Nitrox.Discovery.InstallationFinders;

/// <summary>
///     MS Store games are stored under <c>C:\XboxGames\[GAME]\Content\</c> by default.
///     It's likely we could read the choosen path from <c>C:\Program Files\WindowsApps</c> but we're unable to read store settings from those folders.
/// </summary>
public sealed class MicrosoftFinder : IGameFinder
{
    public IEnumerable<FinderResult> FindGame(FindGameInfo input)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            yield break;
        }

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

        yield return Path.Combine("C:\\", "XboxGames", input.NormalizedGameName, "Content");
        foreach (string logicalDrive in logicalDrives)
        {
            string dirSearch = input.FindFolderWithGameName(Path.Combine(logicalDrive, "XboxGames"));
            if (dirSearch != "")
            {
                yield return Path.Combine(dirSearch, "Content");
            }
        }
    }
}
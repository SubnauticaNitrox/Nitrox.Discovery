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
        try
        {
            // TODO: Read the user defined path from MS Store app
            foreach (string logicalDrive in Directory.GetLogicalDrives())
            {
                string path = Path.Combine(logicalDrive, "XboxGames", gameInfo.Name, "Content");
                if (path.IsDirectoryWithTopLevelExecutable())
                {
                    return Ok(path);
                }
            }
        }
        catch
        {
            string path = Path.Combine("C:\\", "XboxGames", gameInfo.Name, "Content");
            if (path.IsDirectoryWithTopLevelExecutable())
            {
                return Ok(path);
            }
        }

        return NotFound();
    }
}

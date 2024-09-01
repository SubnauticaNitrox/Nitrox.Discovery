using System;
using System.Collections.Generic;
using System.IO;
using Nitrox.Discovery.InstallationFinders.Core;

namespace Nitrox.Discovery.InstallationFinders;

/// <summary>
///     Trying to find install either in appdata or in C:. So for now we just check these 2 paths until we have a better way.
///     Discord stores game files in a subfolder called "content" while the parent folder is used to store Discord related files instead.
/// </summary>
public sealed class DiscordFinder : IGameFinder
{
    public IEnumerable<FinderResult> FindGame(FindGameInfo input)
    {
        string localAppdataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string discordGamesDirectory = Path.Combine(localAppdataDirectory, "DiscordGames");

        yield return Path.Combine(discordGamesDirectory, input.NormalizedGameName, "content");
        yield return Path.Combine("C:\\", "Games", input.NormalizedGameName, "content");
        string dirSearch = input.FindFolderWithGameName(discordGamesDirectory);
        if (dirSearch != "")
        {
            yield return Path.Combine(dirSearch, "content");
        }
    }
}
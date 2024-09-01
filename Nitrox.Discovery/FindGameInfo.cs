using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Nitrox.Discovery;

public sealed record FindGameInfo
{
    public string GameName { get; init; } = "";

    public string ExeName { get; init; } = "";

    public int ExeSearchDepth { get; set; }

    private string normalizedGameName;

    public string NormalizedGameName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(normalizedGameName))
            {
                normalizedGameName = NormalizeGameName(GameName);
            }
            return normalizedGameName;
        }
    }

    public bool IsSimilarGameName(string gameName)
    {
        if (gameName is null or "")
        {
            return false;
        }
        if (GameName.Equals(gameName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        if (NormalizedGameName.Equals(NormalizeGameName(gameName), StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        return false;
    }

    private string NormalizeGameName(string gameName)
    {
        return Regex.Replace(gameName, "[^a-zA-Z0-9_. ]+", "");
    }

    public string FindFolderWithGameName(string rootDirectory)
    {
        try
        {
            foreach (string directory in Directory.EnumerateDirectories(rootDirectory, "*", SearchOption.TopDirectoryOnly))
            {
                if (IsSimilarGameName(Path.GetFileName(directory)))
                {
                    return directory;
                }
            }
        }
        catch (IOException)
        {
            // ignored
        }

        return "";
    }
}
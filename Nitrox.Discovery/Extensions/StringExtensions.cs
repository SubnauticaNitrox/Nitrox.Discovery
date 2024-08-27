using System;
using System.IO;

namespace Nitrox.Discovery.Extensions;

internal static class StringExtensions
{
    /// <summary>
    ///     Gets the relative distance between two paths where the first parameter is a subdirectory of the basePah.
    /// </summary>
    /// <returns>-1 if basePath is not the base, or a positive number if path is a subdirectory to basePath.</returns>
    public static int GetPathDepth(this string path, string basePath)
    {
        path = Path.GetFullPath(path).ToLowerInvariant();
        basePath = Path.GetFullPath(basePath).ToLowerInvariant();
        if (!path.StartsWith(basePath, StringComparison.Ordinal))
        {
            return -1;
        }
        int depth = 0;
        while (!path.Equals(basePath, StringComparison.Ordinal) && path != "")
        {
            path = Path.GetDirectoryName(path) ?? "";
            depth++;
        }

        return path == "" ? -1 : depth;
    }
}

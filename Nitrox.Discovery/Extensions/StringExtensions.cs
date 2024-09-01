using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Nitrox.Discovery.Extensions;

internal static class StringExtensions
{
    /// <summary>
    ///     Gets the relative distance between two paths where the first parameter is a subdirectory of the basePah.
    /// </summary>
    /// <returns>-1 if basePath is not the base, or a positive number if path is a subdirectory to basePath.</returns>
    public static int GetPathDepth(this string path, string basePath)
    {
        StringComparison comparison = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        path = Path.GetFullPath(path);
        basePath = Path.GetFullPath(basePath);
        if (!path.StartsWith(basePath, comparison))
        {
            return -1;
        }
        int depth = 0;
        while (!path.Equals(basePath, comparison) && path != "")
        {
            path = Path.GetDirectoryName(path) ?? "";
            depth++;
        }

        return path == "" ? -1 : depth;
    }

    public static bool IsExecutableFile(this string pathToFile)
    {
        switch (Path.GetExtension(pathToFile)?.ToLowerInvariant())
        {
            case ".exe" or ".com" when RuntimeInformation.IsOSPlatform(OSPlatform.Windows):
            case ".so" or ".o" or null when RuntimeInformation.IsOSPlatform(OSPlatform.Linux):
                return File.Exists(pathToFile);
            default:
                return false;
        }
    }

    public static string ReplaceInvalidFileNameChars(this string fileName, string replacement = "")
    {
        if (fileName == null)
        {
            throw new ArgumentNullException(nameof(fileName));
        }
        return string.Join(replacement, fileName.Split(Path.GetInvalidFileNameChars()));
    }
}
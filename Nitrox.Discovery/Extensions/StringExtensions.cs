using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Nitrox.Discovery.Extensions;

internal static class StringExtensions
{
    /// <summary>
    ///     Gets the relative distance between two paths where the first parameter is a subdirectory of the basePath.
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
        try
        {
            using BinaryReader fs = new(File.OpenRead(pathToFile));
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || Path.GetExtension(pathToFile).Equals(".exe", StringComparison.OrdinalIgnoreCase))
            {
                // MZ (ASCII)
                return fs.ReadBytes(2) is [0x4D, 0x5A];
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // 7F + ELF (ASCII)
                return fs.ReadBytes(4) is [0x7F, 0x45, 0x4C, 0x46];
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // Either 32bit or 64bit program respectively
                return fs.ReadBytes(4) is [0xFE, 0xED, 0xFA, 0xCE] or [0xFE, 0xED, 0xFA, 0xCF];
            }
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }

    /// <summary>
    ///     Replaces common invalid file name characters. On Linux, only 2 characters are invalid, but we use Windows standard as it's more popular.
    /// </summary>
    public static string ReplaceCommonInvalidFileNameChars(this string fileName, string replacement = "")
    {
        if (fileName == null)
        {
            throw new ArgumentNullException(nameof(fileName));
        }
        return string.Join(replacement, fileName.Split('<', '>', '"', '/', '\\', '|', '?', '*', ':', (char)47));
    }
}
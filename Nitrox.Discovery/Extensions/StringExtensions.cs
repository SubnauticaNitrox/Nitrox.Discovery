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
        StringComparison comparison = IsOSPlatform(OSPlatform.Windows) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
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

            byte[] header = fs.ReadBytes(4);

            if (IsOSPlatform(OSPlatform.Windows))
            {
                // MZ (ASCII)
                return header is [0x4D, 0x5A, ..] || Path.GetExtension(pathToFile).Equals(".exe", StringComparison.OrdinalIgnoreCase);
            }
            else if (IsOSPlatform(OSPlatform.Linux))
            {
                // 7F + ELF (ASCII)
                return header is [0x7F, 0x45, 0x4C, 0x46];
            }
            else if (IsOSPlatform(OSPlatform.OSX))
            {
                // Mach-O 32bit (big-endian and little-endian)
                return header is [0xFE, 0xED, 0xFA, 0xCE] or [0xCE, 0xFA, 0xED, 0xFE]
                // Mach-O 64bit (big-endian and little-endian)
                              or [0xFE, 0xED, 0xFA, 0xCF] or [0xCF, 0xFA, 0xED, 0xFE]
                // Fat Mach-O 32bit (big-endian and little-endian)
                              or [0xCA, 0xFE, 0xBA, 0xBE] or [0xBE, 0xBA, 0xFE, 0xCA]
                // Fat Mach-O 64bit (big-endian and little-endian)
                              or [0xCA, 0xFE, 0xBA, 0xBF] or [0xBF, 0xBA, 0xFE, 0xCA];
            }

            // Fallback ?
            return Path.GetExtension(pathToFile).Equals(".exe", StringComparison.OrdinalIgnoreCase);
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
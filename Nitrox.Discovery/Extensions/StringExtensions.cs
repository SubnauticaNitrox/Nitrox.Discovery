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
        byte[] header;
        try
        {
            using BinaryReader fs = new(File.OpenRead(pathToFile));
            header = fs.ReadBytes(4);
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }

        // Check for MZ even on non-Windows platforms because they could be used for Proton/Wine emulation.
        if (IsOSPlatform(OSPlatform.Windows) || Path.GetExtension(pathToFile).Equals(".exe", StringComparison.OrdinalIgnoreCase))
        {
            // MZ (ASCII)
            return header is [0x4D, 0x5A, ..];
        }
        if (IsOSPlatform(OSPlatform.Linux))
        {
            // 7F + ELF (ASCII)
            return header is [0x7F, 0x45, 0x4C, 0x46];
        }
        if (IsOSPlatform(OSPlatform.OSX))
        {
            if (BitConverter.IsLittleEndian)
            {
                return header switch
                {
                    // Mach-O 32bit
                    [0xCE, 0xFA, 0xED, 0xFE] => true,
                    // Mach-O 64bit
                    [0xCF, 0xFA, 0xED, 0xFE] => true,
                    // Fat Mach-O 32bit
                    [0xBE, 0xBA, 0xFE, 0xCA] => true,
                    // Fat Mach-O 64bit
                    [0xBF, 0xBA, 0xFE, 0xCA] => true,
                    _ => false
                };
            }
            return header switch
            {
                // Mach-O 32bit
                [0xFE, 0xED, 0xFA, 0xCE] => true,
                // Mach-O 64bit
                [0xFE, 0xED, 0xFA, 0xCF] => true,
                // Fat Mach-O 32bit
                [0xCA, 0xFE, 0xBA, 0xBE] => true,
                // Fat Mach-O 64bit
                [0xCA, 0xFE, 0xBA, 0xBF] => true,
                _ => false
            };
        }

        return false;
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
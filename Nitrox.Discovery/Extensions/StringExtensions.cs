using System.IO;
using System.Linq;

namespace Nitrox.Discovery.Extensions;

internal static class StringExtensions
{
    public static bool IsDirectoryWithTopLevelExecutable(this string path)
    {
        try
        {
            return Directory.EnumerateFileSystemEntries(path, "*.exe", SearchOption.TopDirectoryOnly).Any();
        }
        catch
        {
            return false;
        }
    }
}

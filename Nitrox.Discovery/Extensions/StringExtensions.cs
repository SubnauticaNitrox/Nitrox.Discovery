using System.IO;
using System.Linq;

namespace Nitrox.Discovery.Extensions;

public static class StringExtensions
{
    public static bool IsDirectoryWithTopLevelExecutable(this string path) => Directory.EnumerateFileSystemEntries(path, "*.exe", SearchOption.TopDirectoryOnly).Any();
}

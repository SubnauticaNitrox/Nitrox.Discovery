using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Build.Framework;

namespace Nitrox.Discovery.MSBuild;

internal static class Extensions
{
    private static bool TryGetMetadata(this ITaskItem taskItem, string metadataName, [NotNullWhen(true)] out string metadata)
    {
        var metadataNames = (ICollection<string>)taskItem.MetadataNames;
        if (metadataNames.Contains(metadataName))
        {
            metadata = taskItem.GetMetadata(metadataName);
            return true;
        }

        metadata = null;
        return false;
    }

    public static string GetGameName(this ITaskItem taskItem)
    {
        if (taskItem.TryGetMetadata("GameName", out var rawGameName))
        {
            return rawGameName;
        }

        return "";
    }
}

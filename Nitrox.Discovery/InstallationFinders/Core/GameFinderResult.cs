using System;
using System.Runtime.CompilerServices;
using Nitrox.Discovery.Models;

namespace Nitrox.Discovery.InstallationFinders.Core;

public sealed record GameFinderResult
{
    internal GameFinderResult()
    {
    }

    public string? ErrorMessage { get; init; }
    public GameLibraries Origin { get; init; }
    public string Path { get; init; } = "";

    /// <summary>
    ///     Gets the name of type that made the result.
    /// </summary>
    public string FinderName { get; init; } = "";

    public static GameFinderResult Error(string message, [CallerFilePath] string callerCodeFile = "") =>
        new()
        {
            FinderName = callerCodeFile[(callerCodeFile.LastIndexOf("\\", StringComparison.Ordinal) + 1)..^3],
            ErrorMessage = message
        };

    private static GameFinderResult Ok(string path, [CallerFilePath] string callerCodeFile = "") =>
        new()
        {
            FinderName = callerCodeFile[(callerCodeFile.LastIndexOf("\\", StringComparison.Ordinal) + 1)..^3],
            Path = path
        };

    public static implicit operator GameFinderResult(string path) => Ok(path);
}
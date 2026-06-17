using System.Runtime.CompilerServices;
using Nitrox.Discovery.Models;
using static System.IO.Path;

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
            FinderName = GetFileNameWithoutExtension(callerCodeFile),
            ErrorMessage = message
        };

    private static GameFinderResult Ok(string path, [CallerFilePath] string callerCodeFile = "") =>
        new()
        {
            FinderName = GetFileNameWithoutExtension(callerCodeFile),
            Path = path
        };

    public static implicit operator GameFinderResult(string? path) => string.IsNullOrWhiteSpace(path) ? Error("Game not found") : Ok(path!);
}
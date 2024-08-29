using System;

namespace Nitrox.Discovery.Models;

[Flags]
public enum GameLibraries
{
    /// <summary>
    ///     Steam
    /// </summary>
    STEAM = 1 << 0,

    /// <summary>
    ///     Epic Games Store
    /// </summary>
    EPIC = 1 << 1,
    EGS = EPIC,

    /// <summary>
    ///     Microsoft store
    /// </summary>
    MICROSOFT = 1 << 2,
    MSSTORE = MICROSOFT,

    /// <summary>
    ///     Games installed using GOG Galaxy
    /// </summary>
    GOG = 1 << 3,

    /// <summary>
    ///     Discord game store
    /// </summary>
    DISCORD = 1 << 4,

    /// <summary>
    ///     Commercial game platforms
    /// </summary>
    PLATFORMS = STEAM | EPIC | MICROSOFT | DISCORD | GOG,

    /// <summary>
    ///     All supported providers
    /// </summary>
    ALL = PLATFORMS
}
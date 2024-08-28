using System;

namespace Nitrox.Discovery.Models;

[Flags]
public enum GameLibraries
{
    /// <summary>
    /// Local config value
    /// </summary>
    CONFIG = 1 << 0,

    /// <summary>
    /// Environment variable value
    /// </summary>
    ENVIRONMENT = 1 << 1,

    /// <summary>
    /// Steam
    /// </summary>
    STEAM = 1 << 2,

    /// <summary>
    /// Epic Games Store
    /// </summary>
    EPIC = 1 << 3,

    /// <summary>
    /// Microsoft store
    /// </summary>
    MICROSOFT = 1 << 4,

    /// <summary>
    /// Discord game store
    /// </summary>
    DISCORD = 1 << 5,

    /// <summary>
    /// Commercial game platforms
    /// </summary>
    PLATFORMS = STEAM | EPIC | MICROSOFT | DISCORD,

    /// <summary>
    /// Custom source that is not a commercial game platform
    /// </summary>
    CUSTOM = CONFIG | ENVIRONMENT,

    /// <summary>
    /// All supported providers
    /// </summary>
    ALL = PLATFORMS | CUSTOM
}

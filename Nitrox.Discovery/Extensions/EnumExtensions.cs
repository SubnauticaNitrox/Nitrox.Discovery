using System;
using System.Collections.Generic;

namespace Nitrox.Discovery.Extensions;

internal static class EnumExtensions
{
    /// <summary>
    ///     Gets only the unique flags of the given enum value that aren't part of a different flag in the same enum type, excluding the 0 flag.
    /// </summary>
    public static IEnumerable<T> GetUniqueNonCombinatoryFlags<T>(this T flags) where T : Enum
    {
        ulong flagCursor = 1;
        ulong aggregate = 0; // Used to deduplicate when the enum has flags have the same value.
        foreach (T value in Enum.GetValues(typeof(T)))
        {
            if (!flags.HasFlag(value))
            {
                continue;
            }

            ulong definedFlagBits = Convert.ToUInt64(value);
            while (flagCursor < definedFlagBits)
            {
                flagCursor <<= 1;
            }

            if (flagCursor == definedFlagBits && (aggregate & flagCursor) == 0 && value.HasFlag(value))
            {
                aggregate |= flagCursor;
                yield return value;
            }
        }
    }

    /// <inheritdoc cref="Enum.IsDefined" />
    public static bool IsDefined<TEnum>(this TEnum value) where TEnum : Enum => Enum.IsDefined(typeof(TEnum), value);
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace NETworkManager.Utilities;

/// <summary>
///     Completes the last entry of a list separated by ";" with a name (e.g. "webserver; dat" -> "webserver; Database"),
///     used by the Tab completion of an input (e.g. the ports of the Port Scanner with port profile names).
/// </summary>
public static class ListEntryCompletionHelper
{
    /// <summary>
    ///     Splits a list into the text before the last entry (as entered, with the separator and the whitespace after
    ///     it) and the last entry (e.g. "webserver; dat" -> "webserver; " and "dat").
    /// </summary>
    /// <param name="text">List separated by ";".</param>
    /// <returns>Text before the last entry and the last entry.</returns>
    public static (string prefix, string entry) SplitLastEntry(string text)
    {
        text ??= string.Empty;

        var start = text.LastIndexOf(';') + 1;

        while (start < text.Length && char.IsWhiteSpace(text[start]))
            start++;

        return (text[..start], text[start..]);
    }

    /// <summary>
    ///     Gets the names an entry can be completed with: names that start with the entry first, then names that
    ///     contain it (case-insensitive, each sorted by name, every name once).
    /// </summary>
    /// <param name="entry">Entry that is being typed.</param>
    /// <param name="names">Available names.</param>
    /// <returns>Names, empty if the entry is empty or nothing matches.</returns>
    public static List<string> GetCandidates(string entry, IEnumerable<string> names)
    {
        var typed = entry?.Trim();

        if (string.IsNullOrEmpty(typed))
            return [];

        var distinctNames = names.Where(x => !string.IsNullOrEmpty(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return
        [
            .. distinctNames.Where(x => x.StartsWith(typed, StringComparison.OrdinalIgnoreCase)),
            .. distinctNames.Where(x => !x.StartsWith(typed, StringComparison.OrdinalIgnoreCase) &&
                                        x.Contains(typed, StringComparison.OrdinalIgnoreCase))
        ];
    }
}

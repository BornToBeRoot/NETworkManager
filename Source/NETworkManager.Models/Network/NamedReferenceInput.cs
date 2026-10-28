using System;
using System.Collections.Generic;
using System.Linq;

namespace NETworkManager.Models.Network;

/// <summary>
///     Item that can be entered by its name in an input with several entries (e.g. a port profile in a ports input)
///     and is saved as a reference to its ID (see <see cref="NamedReferenceInput" />).
/// </summary>
public interface INamedReference
{
    /// <summary>
    ///     Internal ID, kept when the item is renamed.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    ///     Name that is entered in the input.
    /// </summary>
    string Name { get; }
}

/// <summary>
///     Input with entries separated by ";" (e.g. "Webserver; 8080"), where an entry can be the name of an item (e.g. a
///     port profile or DNS server profile). The names can be replaced by references to the ID of the item (e.g.
///     "{8f1c...}; 8080"), so a profile still finds the item after it was renamed.
/// </summary>
public static class NamedReferenceInput
{
    /// <summary>
    ///     Separator of the entries of an input.
    /// </summary>
    public const char EntrySeparator = ';';

    /// <summary>
    ///     Finds an item by its name (case-insensitive, leading / trailing spaces are ignored).
    /// </summary>
    /// <param name="items">Items.</param>
    /// <param name="name">Name of the item.</param>
    /// <returns>Item, or null if there is none with the name.</returns>
    public static T FindByName<T>(IEnumerable<T> items, string name) where T : class, INamedReference
    {
        var trimmedName = name?.Trim();

        return string.IsNullOrEmpty(trimmedName)
            ? null
            : items.FirstOrDefault(x => string.Equals(x.Name?.Trim(), trimmedName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    ///     Replaces the names of items in an input by references to their ID (e.g. "Webserver; 8080" ->
    ///     "{8f1c...}; 8080"). Entries without an item (or an item without an ID) are kept, trimmed.
    /// </summary>
    /// <param name="input">Input.</param>
    /// <param name="findItem">Gets the item an entry is replaced by, null to keep the entry.</param>
    /// <returns>Input with references, null if it contains no item.</returns>
    public static string EncodeReferences<T>(string input, Func<string, T> findItem) where T : class, INamedReference
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        var hasReference = false;
        List<string> entries = [];

        foreach (var entry in input.Split(EntrySeparator))
        {
            var item = findItem(entry);

            if (item == null || item.Id == Guid.Empty)
            {
                entries.Add(entry.Trim());
                continue;
            }

            entries.Add(item.Id.ToString("B"));
            hasReference = true;
        }

        return hasReference ? string.Join("; ", entries) : null;
    }

    /// <summary>
    ///     Replaces the references in an input (see <see cref="EncodeReferences{T}" />) by the current names of the
    ///     items (e.g. "{8f1c...}; 8080" -> "Webserver; 8080").
    /// </summary>
    /// <param name="inputWithReferences">Input with references.</param>
    /// <param name="items">Items.</param>
    /// <param name="input">Input with the names of the items, null if an item was not found.</param>
    /// <returns>True if every referenced item was found.</returns>
    public static bool TryDecodeReferences<T>(string inputWithReferences, IEnumerable<T> items, out string input)
        where T : class, INamedReference
    {
        input = null;

        if (string.IsNullOrWhiteSpace(inputWithReferences))
            return false;

        var itemList = items as IList<T> ?? [.. items];
        List<string> entries = [];

        foreach (var entry in inputWithReferences.Split(EntrySeparator))
        {
            if (!Guid.TryParseExact(entry.Trim(), "B", out var id))
            {
                entries.Add(entry.Trim());
                continue;
            }

            var item = itemList.FirstOrDefault(x => x.Id == id);

            if (item == null)
                return false;

            entries.Add(item.Name);
        }

        input = string.Join("; ", entries);

        return true;
    }
}

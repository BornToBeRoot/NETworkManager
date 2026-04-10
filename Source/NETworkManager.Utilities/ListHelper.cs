using System;
using System.Collections.Generic;

namespace NETworkManager.Utilities;
/// <summary>
/// Helper class for modifying lists by adding new entries and removing old ones if the list exceeds a specified length.
/// </summary>
public static class ListHelper
{
    /// <summary>
    /// Modify a list by adding the <paramref name="entry"/> and removing the oldest entry if the list is full.
    /// If an entry or multiple ones already exist in the list, they will be removed before adding the new entry.
    /// </summary>
    /// <param name="list">List to modify. Used with <paramref name="entry"/> to add and remove entries.</param>
    /// <param name="entry">Entry to add to the list.</param>
    /// <param name="length">Maximum length of the list. Oldest entries will be removed if the list exceeds this length.</param>
    /// <typeparam name="T">Type of the list entries. Currently <see cref="string"/> or <see cref="int"/>.</typeparam>
    /// <returns>Modified list with the new entry added and oldest entries removed if necessary.</returns>
    public static List<T> Modify<T>(List<T> list, T entry, int length)
    {
        int index;

        while ((index = list.IndexOf(entry)) != -1)
        {
            list.RemoveAt(index);
        }

        if (length <= 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than zero.");

        while (list.Count >= length)
            list.RemoveAt(list.Count - 1);

        list.Insert(0, entry);

        return list;
    }
}

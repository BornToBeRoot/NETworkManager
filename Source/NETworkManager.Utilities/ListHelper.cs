using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NETworkManager.Utilities;
/// <summary>
/// Helper class for history lists: add an entry as the newest one and remove the oldest ones if the list exceeds a
/// specified length.
/// </summary>
public static class ListHelper
{
    /// <summary>
    /// Adds the <paramref name="entry"/> as the newest (first) entry of the list. An existing entry is moved to the top
    /// (duplicates are removed) and the oldest entries are removed if the list exceeds the <paramref name="length"/>.
    /// The list is changed in place, so controls bound to it (e.g. the history of a ComboBox) keep their text and are
    /// notified only about the actual changes. Default values (null, 0) and empty strings are not added (e.g. optional
    /// inputs).
    /// </summary>
    /// <param name="list">List to modify (e.g. a history of the settings).</param>
    /// <param name="entry">Entry to add to the list.</param>
    /// <param name="length">Maximum length of the list. Oldest entries will be removed if the list exceeds this length.</param>
    /// <typeparam name="T">Type of the list entries. Currently <see cref="string"/> or <see cref="int"/>.</typeparam>
    public static void Modify<T>(ObservableCollection<T> list, T entry, int length)
    {
        if (length <= 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than zero.");

        if (EqualityComparer<T>.Default.Equals(entry, default) || entry is string text && string.IsNullOrEmpty(text))
            return;

        var index = list.IndexOf(entry);

        if (index > 0)
            list.Move(index, 0);
        else if (index < 0)
            list.Insert(0, entry);

        // Remove duplicates (e.g. from older settings files)
        for (var i = list.Count - 1; i > 0; i--)
        {
            if (EqualityComparer<T>.Default.Equals(list[i], entry))
                list.RemoveAt(i);
        }

        while (list.Count > length)
            list.RemoveAt(list.Count - 1);
    }
}

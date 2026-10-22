using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace NETworkManager.Controls;

/// <summary>
/// <see cref="ObservableCollection{T}"/> that can replace all items at once with a single
/// <see cref="NotifyCollectionChangedAction.Reset"/> notification (instead of one notification per item).
/// Bound views (e.g. a filtered / sorted <see cref="System.ComponentModel.ICollectionView"/>) are refreshed only once,
/// which is much faster for large lists.
/// </summary>
/// <typeparam name="T">The type of the items.</typeparam>
public class ObservableRangeCollection<T> : ObservableCollection<T>
{
    /// <summary>
    /// Replaces all items with the given items and raises a single
    /// <see cref="NotifyCollectionChangedAction.Reset"/> notification.
    /// </summary>
    /// <param name="items">The new items.</param>
    public void ReplaceAll(IEnumerable<T> items)
    {
        CheckReentrancy();

        // Read the items first: if a lazy sequence throws, the collection is unchanged (and in sync with the views).
        var newItems = items.ToList();

        Items.Clear();

        foreach (var item in newItems)
            Items.Add(item);

        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace NETworkManager.Utilities.WPF;

/// <summary>
///     Extension methods for <see cref="ICollectionView" />, e.g. to export or copy the rows of a grid in the order they
///     are shown.
/// </summary>
public static class CollectionViewExtensions
{
    /// <summary>
    ///     Gets the items of the view in the order they are shown: filtered, sorted and grouped (the items of the groups
    ///     in the order of the groups, also nested groups). Call it on the UI thread (the view is not thread-safe).
    /// </summary>
    /// <typeparam name="T">Type of the items.</typeparam>
    /// <param name="view">View of the items (e.g. the view a grid is bound to).</param>
    /// <returns>Items in the order shown.</returns>
    public static List<T> GetItemsInViewOrder<T>(this ICollectionView view)
    {
        // Enumerating a grouped view returns the items in the sort order, not grouped
        return view.Groups is { Count: > 0 } groups ? GetItems<T>(groups).ToList() : view.Cast<T>().ToList();
    }

    /// <summary>
    ///     Gets the selected items in the order they are shown in the view (the selected items of a grid are in the
    ///     order they were selected). Call it on the UI thread.
    /// </summary>
    /// <typeparam name="T">Type of the items.</typeparam>
    /// <param name="view">View of the items (e.g. the view a grid is bound to).</param>
    /// <param name="selectedItems">Selected items (e.g. <c>DataGrid.SelectedItems</c>).</param>
    /// <returns>Selected items in the order shown, without items that are not shown (e.g. filtered).</returns>
    public static List<T> GetSelectedItemsInViewOrder<T>(this ICollectionView view, IEnumerable selectedItems)
    {
        // By reference: two equal items (e.g. records) are still two rows, only the selected one is returned
        var selected = new HashSet<object>(selectedItems.Cast<object>(), ReferenceEqualityComparer.Instance);

        return view.GetItemsInViewOrder<T>().Where(item => selected.Contains(item)).ToList();
    }

    /// <summary>
    ///     Gets all items or only the selected items in the order they are shown, e.g. for <b>Export...</b> > All /
    ///     Selected. Call it on the UI thread.
    /// </summary>
    /// <typeparam name="T">Type of the items.</typeparam>
    /// <param name="view">View of the items (e.g. the view a grid is bound to).</param>
    /// <param name="all">True for all items shown, false for the selected items.</param>
    /// <param name="selectedItems">Selected items (e.g. <c>DataGrid.SelectedItems</c>), only used if not all.</param>
    /// <returns>Items in the order shown.</returns>
    public static List<T> GetItemsInViewOrder<T>(this ICollectionView view, bool all, IEnumerable selectedItems)
    {
        return all ? view.GetItemsInViewOrder<T>() : view.GetSelectedItemsInViewOrder<T>(selectedItems);
    }

    /// <summary>
    ///     Gets the items of groups (recursive for nested groups).
    /// </summary>
    private static IEnumerable<T> GetItems<T>(IEnumerable groupsOrItems)
    {
        foreach (var item in groupsOrItems)
        {
            if (item is CollectionViewGroup group)
            {
                foreach (var groupItem in GetItems<T>(group.Items))
                    yield return groupItem;
            }
            else
            {
                yield return (T)item;
            }
        }
    }
}

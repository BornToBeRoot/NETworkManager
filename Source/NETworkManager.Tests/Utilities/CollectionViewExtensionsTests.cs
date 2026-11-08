using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using NETworkManager.Utilities.WPF;
using Xunit;

namespace NETworkManager.Tests.Utilities;

/// <summary>
///     Tests of <see cref="CollectionViewExtensions" /> (rows of a grid in the order shown, e.g. for the export). The
///     views need WPF: the tests only run on Windows. The WPF code is in separate methods, so the test methods can be
///     loaded without WPF.
/// </summary>
public class CollectionViewExtensionsTests
{
    private sealed record Item(string Group, int Value);

    private static readonly Item[] Items =
    [
        new("b", 3), new("a", 2), new("b", 1), new("a", 4), new("c", 5)
    ];

    private static void SkipUnlessWindows()
    {
        Assert.SkipUnless(OperatingSystem.IsWindows(), "Needs WPF (Windows)");
    }

    [Fact]
    public void GetItemsInViewOrder_Unsorted_SourceOrder()
    {
        SkipUnlessWindows();

        Assert.Equal([3, 2, 1, 4, 5], GetItems(sort: false, filter: false, group: false));
    }

    [Fact]
    public void GetItemsInViewOrder_SortedAndFiltered()
    {
        SkipUnlessWindows();

        // Sorted by value, values > 4 filtered
        Assert.Equal([1, 2, 3, 4], GetItems(sort: true, filter: true, group: false));
    }

    [Fact]
    public void GetItemsInViewOrder_Grouped_InGroupOrder()
    {
        SkipUnlessWindows();

        // Groups sorted by name, items sorted by value within the group
        Assert.Equal([2, 4, 1, 3, 5], GetItems(sort: true, filter: false, group: true));
    }

    [Fact]
    public void GetSelectedItemsInViewOrder_ViewOrderWithoutFilteredItems()
    {
        SkipUnlessWindows();

        // Selected in the order 5 (filtered), 4, 1, 3 - returned in the order shown
        Assert.Equal([1, 3, 4], GetSelectedItems([5, 4, 1, 3]));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static List<int> GetItems(bool sort, bool filter, bool group)
    {
        return CreateView(sort, filter, group).GetItemsInViewOrder<Item>().Select(x => x.Value).ToList();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static List<int> GetSelectedItems(int[] selectedValues)
    {
        var selected = selectedValues.Select(value => Items.Single(x => x.Value == value)).ToList();

        return CreateView(true, true, false).GetSelectedItemsInViewOrder<Item>(selected).Select(x => x.Value).ToList();
    }

    private static ListCollectionView CreateView(bool sort, bool filter, bool group)
    {
        var view = new ListCollectionView(Items.ToList());

        if (group)
        {
            view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Item.Group)));
            view.SortDescriptions.Add(new SortDescription(nameof(Item.Group), ListSortDirection.Ascending));
        }

        if (sort)
            view.SortDescriptions.Add(new SortDescription(nameof(Item.Value), ListSortDirection.Ascending));

        if (filter)
            view.Filter = x => ((Item)x).Value <= 4;

        return view;
    }
}

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using NETworkManager.Utilities;
using Xunit;

namespace NETworkManager.Tests.Utilities;

public class ListHelperTests
{
    [Fact]
    public void AddsNewEntryAtTheTop()
    {
        ObservableCollection<string> list = ["b", "c"];

        ListHelper.Modify(list, "a", 5);

        Assert.Equal(["a", "b", "c"], list);
    }

    [Fact]
    public void MovesExistingEntryToTheTop()
    {
        ObservableCollection<string> list = ["a", "b", "c"];

        ListHelper.Modify(list, "c", 5);

        Assert.Equal(["c", "a", "b"], list);
    }

    [Fact]
    public void RemovesOldestEntriesBeyondTheLength()
    {
        ObservableCollection<string> list = ["b", "c", "d"];

        ListHelper.Modify(list, "a", 3);

        Assert.Equal(["a", "b", "c"], list);
    }

    [Fact]
    public void ShortensAListThatIsTooLong()
    {
        // E.g. after the history length was reduced in the settings.
        ObservableCollection<string> list = ["a", "b", "c", "d"];

        ListHelper.Modify(list, "a", 2);

        Assert.Equal(["a", "b"], list);
    }

    [Fact]
    public void RemovesDuplicatesOfTheEntry()
    {
        ObservableCollection<int> list = [1, 2, 3, 2];

        ListHelper.Modify(list, 2, 10);

        Assert.Equal([2, 1, 3], list);
    }

    [Fact]
    public void DoesNotChangeTheListIfTheEntryIsAlreadyTheNewest()
    {
        ObservableCollection<string> list = ["a", "b"];
        var changes = 0;
        list.CollectionChanged += (_, _) => changes++;

        ListHelper.Modify(list, "a", 5);

        Assert.Equal(0, changes);
    }

    [Fact]
    public void MovesInsteadOfResetting()
    {
        // A reset (Clear) would remove the entry that a ComboBox shows, which can clear its text.
        ObservableCollection<string> list = ["a", "b", "c"];
        var actions = new System.Collections.Generic.List<NotifyCollectionChangedAction>();
        list.CollectionChanged += (_, e) => actions.Add(e.Action);

        ListHelper.Modify(list, "b", 5);

        Assert.Equal([NotifyCollectionChangedAction.Move], actions);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IgnoresEmptyEntries(string entry)
    {
        ObservableCollection<string> list = ["a"];

        ListHelper.Modify(list, entry, 5);

        Assert.Equal(["a"], list);
    }

    [Fact]
    public void IgnoresDefaultValue()
    {
        // E.g. a port of 0 if the port is not used.
        ObservableCollection<int> list = [22];

        ListHelper.Modify(list, 0, 5);

        Assert.Equal([22], list);
    }

    [Fact]
    public void UsesTheComparerAndKeepsTheLatestSpelling()
    {
        ObservableCollection<string> list = ["a", "ping 10.0.0.1"];

        ListHelper.Modify(list, "Ping 10.0.0.1", 5, StringComparer.OrdinalIgnoreCase);

        Assert.Equal(["Ping 10.0.0.1", "a"], list);
    }

    // "Number of stored entries" can be set to 0 in the settings.
    [Fact]
    public void LengthZeroStoresNoEntries()
    {
        var list = new ObservableCollection<string> { "a", "b" };

        ListHelper.Modify(list, "c", 0);

        Assert.Empty(list);
    }

    [Fact]
    public void ThrowsIfTheLengthIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ListHelper.Modify(new ObservableCollection<string>(), "a", -1));
    }
}

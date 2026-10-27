using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Settings;

namespace NETworkManager;

/// <summary>
///     Entry of the ports input of the Port Scanner: a port profile (with its ports) or an entry of the history.
/// </summary>
/// <param name="Text">Text inserted into the input: name of the port profile or the history entry.</param>
/// <param name="Ports">Ports of the port profile, null for a history entry.</param>
/// <param name="Group">Group in the list (port profiles, history).</param>
public sealed record PortScannerPortsSuggestion(string Text, string Ports, string Group)
{
    /// <summary>
    ///     The text, e.g. kept in an editable ComboBox when the entry is removed from the list (see
    ///     <see cref="ComboBoxHistoryBehavior" />).
    /// </summary>
    public override string ToString()
    {
        return Text;
    }
}

/// <summary>
///     Port profiles and history of the Port Scanner for the ports input (an editable ComboBox like the DNS server of
///     DNS Lookup): the port profiles (sorted by name) and the history, grouped. Updated when the port profiles or the
///     history change (weak events, the list doesn't keep a closed tab or dialog alive).
/// </summary>
public sealed class PortScannerPortsSuggestions
{
    private readonly ObservableRangeCollection<PortScannerPortsSuggestion> _suggestions = [];

    /// <summary>
    ///     Creates the list with the current port profiles and history.
    /// </summary>
    public PortScannerPortsSuggestions()
    {
        View = new CollectionViewSource { Source = _suggestions }.View;
        View.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PortScannerPortsSuggestion.Group)));

        CollectionChangedEventManager.AddHandler(SettingsManager.Current.PortScanner_PortProfiles, OnSourceChanged);
        CollectionChangedEventManager.AddHandler(SettingsManager.Current.PortScanner_PortHistory, OnSourceChanged);

        Refresh();
    }

    /// <summary>
    ///     Gets the view of the entries, grouped (port profiles, history).
    /// </summary>
    public ICollectionView View { get; }

    /// <summary>
    ///     Gets the names of the port profiles, for the Tab completion of an entry (see
    ///     <see cref="ComboBoxListCompletionBehavior" />).
    /// </summary>
    public ObservableRangeCollection<string> PortProfileNames { get; } = [];

    private void OnSourceChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        Refresh();
    }

    private void Refresh()
    {
        PortProfileNames.ReplaceAll(SettingsManager.Current.PortScanner_PortProfiles.Select(x => x.Name));

        _suggestions.ReplaceAll(SettingsManager.Current.PortScanner_PortProfiles
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(x => new PortScannerPortsSuggestion(x.Name, x.Ports, Strings.PortProfiles))
            .Concat(SettingsManager.Current.PortScanner_PortHistory
                .Select(x => new PortScannerPortsSuggestion(x, null, Strings.History))));
    }
}

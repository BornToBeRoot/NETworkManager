using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Settings;

namespace NETworkManager;

/// <summary>
///     Entry of an input with profiles and history (e.g. the ports of the Port Scanner): a profile (with a description,
///     e.g. its ports) or an entry of the history.
/// </summary>
/// <param name="Text">Text inserted into the input: name of the profile or the history entry.</param>
/// <param name="Description">Description of the profile (e.g. its ports), null for a history entry.</param>
/// <param name="Group">Group in the list (profiles, history).</param>
public sealed record InputSuggestion(string Text, string Description, string Group)
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
///     Profiles and history for an input (an editable ComboBox like the ports of the Port Scanner or the DNS server of
///     DNS Lookup): the profiles and the history, grouped, and the names of the profiles for the Tab completion.
///     Updated when the profiles or the history change (weak events, the list doesn't keep a closed tab or dialog
///     alive).
/// </summary>
public sealed class InputSuggestions
{
    private readonly ObservableRangeCollection<InputSuggestion> _suggestions = [];
    private readonly Func<IEnumerable<(string Name, string Description)>> _getProfiles;
    private readonly string _profilesGroup;
    private readonly ObservableCollection<string> _history;

    /// <summary>
    ///     Creates the list with the current profiles and history.
    /// </summary>
    /// <param name="profiles">Collection of the profiles (to update the list when it changes).</param>
    /// <param name="getProfiles">Gets the names and descriptions of the profiles, in the order shown.</param>
    /// <param name="profilesGroup">Name of the group of the profiles.</param>
    /// <param name="history">History of the input.</param>
    private InputSuggestions(INotifyCollectionChanged profiles,
        Func<IEnumerable<(string Name, string Description)>> getProfiles, string profilesGroup,
        ObservableCollection<string> history)
    {
        _getProfiles = getProfiles;
        _profilesGroup = profilesGroup;
        _history = history;

        View = new CollectionViewSource { Source = _suggestions }.View;
        View.GroupDescriptions.Add(new PropertyGroupDescription(nameof(InputSuggestion.Group)));

        CollectionChangedEventManager.AddHandler(profiles, OnSourceChanged);
        CollectionChangedEventManager.AddHandler(history, OnSourceChanged);

        Refresh();
    }

    /// <summary>
    ///     Creates the list for the ports of the Port Scanner: the port profiles (sorted by name, with their ports) and
    ///     the ports history.
    /// </summary>
    public static InputSuggestions ForPortScannerPorts()
    {
        return new InputSuggestions(SettingsManager.Current.PortScanner_PortProfiles,
            () => SettingsManager.Current.PortScanner_PortProfiles
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Select(x => (x.Name, x.Ports)),
            Strings.PortProfiles, SettingsManager.Current.PortScanner_PortHistory);
    }

    /// <summary>
    ///     Creates the list for the DNS server of DNS Lookup: the DNS servers (Windows DNS first, then sorted by name,
    ///     with their servers) and the DNS server history.
    /// </summary>
    public static InputSuggestions ForDNSLookupDNSServers()
    {
        return new InputSuggestions(SettingsManager.Current.DNSLookup_DNSServers,
            () => SettingsManager.Current.DNSLookup_DNSServers
                .OrderByDescending(x => x.UseWindowsDNSServer)
                .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Select(x => (x.Name, x.UseWindowsDNSServer
                    ? null
                    : string.Join("; ", x.Servers.Select(server => server.Port == 53 ? server.Server : server.ToString())))),
            Strings.DNSServers, SettingsManager.Current.DNSLookup_DNSServerHistory);
    }

    /// <summary>
    ///     Gets the view of the entries, grouped (profiles, history).
    /// </summary>
    public ICollectionView View { get; }

    /// <summary>
    ///     Gets the names of the profiles, for the Tab completion of an entry (see
    ///     <see cref="ComboBoxListCompletionBehavior" />).
    /// </summary>
    public ObservableRangeCollection<string> ProfileNames { get; } = [];

    private void OnSourceChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        Refresh();
    }

    private void Refresh()
    {
        var profiles = _getProfiles().ToList();

        ProfileNames.ReplaceAll(profiles.Select(x => x.Name));

        _suggestions.ReplaceAll(profiles
            .Select(x => new InputSuggestion(x.Name, x.Description, _profilesGroup))
            .Concat(_history.Select(x => new InputSuggestion(x, null, Strings.History))));
    }
}

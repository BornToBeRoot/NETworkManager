using System;
using System.Collections.Generic;

namespace NETworkManager.Models.Network;

/// <summary>
///     Result of <see cref="WiFiScanComparer.Compare" />.
/// </summary>
/// <param name="Changes">
///     Networks of the current scan with a change (<see cref="WiFiNetworkChange.New" /> or
///     <see cref="WiFiNetworkChange.Changed" />) and the network of the previous scan (null if new).
/// </param>
/// <param name="Gone">Networks of the previous scan that are not visible now (in the order of the previous scan).</param>
public sealed record WiFiScanComparison(
    IReadOnlyList<(WiFiNetworkInfo Network, WiFiNetworkChange Change, WiFiNetworkInfo Previous)> Changes,
    IReadOnlyList<WiFiNetworkInfo> Gone);

/// <summary>
///     Compares a scan of the WiFi networks with the previous scan of the same adapter, by BSSID: which networks are
///     new, gone or changed their channel, channel width or security. The signal strength is not a change (it varies
///     with every scan).
/// </summary>
public static class WiFiScanComparer
{
    /// <summary>
    ///     Compares the current scan with the previous one.
    /// </summary>
    /// <param name="previous">Networks of the previous scan.</param>
    /// <param name="current">Networks of the current scan.</param>
    /// <returns>Changes of the current networks and the gone networks.</returns>
    public static WiFiScanComparison Compare(IReadOnlyList<WiFiNetworkInfo> previous,
        IReadOnlyList<WiFiNetworkInfo> current)
    {
        var previousByBssid = ByBssid(previous);
        var currentByBssid = ByBssid(current);

        var changes = new List<(WiFiNetworkInfo, WiFiNetworkChange, WiFiNetworkInfo)>();
        var gone = new List<WiFiNetworkInfo>();

        foreach (var network in currentByBssid.Values)
        {
            if (!previousByBssid.TryGetValue(network.Bssid, out var previousNetwork))
                changes.Add((network, WiFiNetworkChange.New, null));
            else if (IsChanged(previousNetwork, network))
                changes.Add((network, WiFiNetworkChange.Changed, previousNetwork));
        }

        foreach (var network in previousByBssid.Values)
        {
            if (!currentByBssid.ContainsKey(network.Bssid))
                gone.Add(network);
        }

        return new WiFiScanComparison(changes, gone);
    }

    /// <summary>
    ///     The channel, the channel width or the security changed. A width that was estimated in one of the scans is not
    ///     compared (it differs if the beacon could be read in one scan only).
    /// </summary>
    private static bool IsChanged(WiFiNetworkInfo previous, WiFiNetworkInfo current)
    {
        return previous.Channel != current.Channel ||
               (!previous.IsChannelBandwidthEstimated && !current.IsChannelBandwidthEstimated &&
                previous.ChannelBandwidth != current.ChannelBandwidth) ||
               !string.Equals(previous.NetworkAuthenticationType, current.NetworkAuthenticationType,
                   StringComparison.Ordinal);
    }

    /// <summary>
    ///     Networks by BSSID, in their order (the first one if a BSSID is reported twice).
    /// </summary>
    private static Dictionary<string, WiFiNetworkInfo> ByBssid(IEnumerable<WiFiNetworkInfo> networks)
    {
        var result = new Dictionary<string, WiFiNetworkInfo>(StringComparer.OrdinalIgnoreCase);

        foreach (var network in networks)
            if (!string.IsNullOrEmpty(network.Bssid))
                result.TryAdd(network.Bssid, network);

        return result;
    }
}

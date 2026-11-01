using System;
using System.Collections.Generic;
using System.Net;

namespace NETworkManager.Models.Network;

/// <summary>
///     A host of a completed scan, the baseline of the next scan of the same input (see
///     <see cref="IPScannerScanComparer" />).
/// </summary>
/// <param name="IsUp">The host was up (reachable).</param>
/// <param name="Hostname">Shown hostname (DNS, mDNS or NetBIOS), empty if unknown.</param>
/// <param name="MACAddress">MAC address, empty if unknown.</param>
/// <param name="Vendor">Vendor of the MAC address, empty if unknown.</param>
public sealed record IPScannerBaselineHost(bool IsUp, string Hostname, string MACAddress, string Vendor);

/// <summary>
///     Result of <see cref="IPScannerScanComparer.Compare" />.
/// </summary>
/// <param name="Changes">Change of each host of the current scan (only hosts with a change).</param>
/// <param name="Gone">Hosts that were up in the previous scan and are not up (or not in the result) now.</param>
public sealed record IPScannerComparison(
    IReadOnlyDictionary<IPAddress, (IPScannerHostChange Change, string PreviousMACAddress)> Changes,
    IReadOnlyList<(IPAddress IPAddress, IPScannerBaselineHost Previous)> Gone);

/// <summary>
///     Compares a scan with the previous scan of the same input: which hosts are new, gone or have another MAC address.
/// </summary>
public static class IPScannerScanComparer
{
    /// <summary>
    ///     Creates the baseline of a completed scan, to compare the next scan with.
    /// </summary>
    /// <param name="hosts">Hosts of the result (up, and down ones if they are shown).</param>
    /// <returns>Baseline by IP address.</returns>
    public static Dictionary<IPAddress, IPScannerBaselineHost> CreateBaseline(IEnumerable<IPScannerHostInfo> hosts)
    {
        var baseline = new Dictionary<IPAddress, IPScannerBaselineHost>();

        foreach (var host in hosts)
        {
            baseline[host.IPAddress] = new IPScannerBaselineHost(host.IsReachable, host.Hostname ?? string.Empty,
                host.MACAddress ?? string.Empty, host.Vendor ?? string.Empty);
        }

        return baseline;
    }

    /// <summary>
    ///     Compares the current scan with the baseline of the previous one.
    /// </summary>
    /// <param name="baseline">Baseline of the previous scan (see <see cref="CreateBaseline" />).</param>
    /// <param name="current">Current scan as baseline (see <see cref="CreateBaseline" />).</param>
    /// <returns>Changes of the current hosts and the gone hosts (in the order of the previous scan).</returns>
    public static IPScannerComparison Compare(IReadOnlyDictionary<IPAddress, IPScannerBaselineHost> baseline,
        IReadOnlyDictionary<IPAddress, IPScannerBaselineHost> current)
    {
        var changes = new Dictionary<IPAddress, (IPScannerHostChange, string)>();
        var gone = new List<(IPAddress, IPScannerBaselineHost)>();

        foreach (var (ipAddress, host) in current)
        {
            if (!host.IsUp)
                continue;

            if (!baseline.TryGetValue(ipAddress, out var previous) || !previous.IsUp)
            {
                changes[ipAddress] = (IPScannerHostChange.New, string.Empty);
                continue;
            }

            if (!string.IsNullOrEmpty(host.MACAddress) && !string.IsNullOrEmpty(previous.MACAddress) &&
                !string.Equals(host.MACAddress, previous.MACAddress, StringComparison.OrdinalIgnoreCase))
                changes[ipAddress] = (IPScannerHostChange.MACChanged, previous.MACAddress);
        }

        foreach (var (ipAddress, previous) in baseline)
        {
            if (previous.IsUp && (!current.TryGetValue(ipAddress, out var host) || !host.IsUp))
                gone.Add((ipAddress, previous));
        }

        return new IPScannerComparison(changes, gone);
    }
}

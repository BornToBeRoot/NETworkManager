using System;
using System.Collections.Generic;

namespace NETworkManager.Models.Network;

/// <summary>
///     Decides if a host that didn't answer any probe of the <see cref="IPScanner" /> is reachable based on its
///     neighbor entry (ARP / NDP). Every probe (ping, TCP, NetBIOS) makes Windows resolve the MAC address of a host in
///     a local subnet first, and a host answers ARP requests / neighbor solicitations even if its firewall drops
///     everything else.
/// </summary>
public static class IPScannerNeighborCheck
{
    /// <summary>
    ///     Checks the neighbor entry of a host that didn't answer any probe.
    /// </summary>
    /// <param name="entry">
    ///     Neighbor entry of the host (read after its probes were finished), null if there is none.
    /// </param>
    /// <param name="isGateway">The host is a default gateway of a local network interface.</param>
    /// <param name="gatewayMACAddresses">MAC addresses of the default gateways (e.g. "001122AABBCC").</param>
    /// <returns>What the entry says about the host.</returns>
    public static IPScannerNeighborCheckResult Check(NeighborInfo entry, bool isGateway,
        IReadOnlyCollection<string> gatewayMACAddresses)
    {
        // No entry (the neighbor table only contains entries with a MAC address): the host didn't answer the ARP
        // request / neighbor solicitation
        if (entry == null)
            return IPScannerNeighborCheckResult.Unreachable;

        // A router with proxy ARP answers for addresses of other hosts (e.g. every address of the subnet) with its own
        // MAC address - not an answer of the host
        if (!isGateway && IsProxyARPAnswer(entry.MACAddress.ToString(), gatewayMACAddresses))
            return IPScannerNeighborCheckResult.Unreachable;

        return entry.State switch
        {
            // Confirmed recently, i.e. when the probes resolved it
            NeighborState.Reachable => IPScannerNeighborCheckResult.Reachable,
            // Entry from earlier traffic, not verified again yet (Delay / Probe: the verification is still running)
            NeighborState.Stale or NeighborState.Delay or NeighborState.Probe => IPScannerNeighborCheckResult.Verify,
            // Static entry (says nothing about the host), or the neighbor didn't answer
            _ => IPScannerNeighborCheckResult.Unreachable
        };
    }

    /// <summary>
    ///     Checks if a MAC address resolved for a host is the MAC address of a default gateway (proxy ARP).
    /// </summary>
    /// <param name="macAddress">Resolved MAC address (e.g. "001122AABBCC").</param>
    /// <param name="gatewayMACAddresses">MAC addresses of the default gateways (e.g. "001122AABBCC").</param>
    /// <returns>True if the MAC address is the MAC address of a default gateway.</returns>
    public static bool IsProxyARPAnswer(string macAddress, IReadOnlyCollection<string> gatewayMACAddresses)
    {
        foreach (var gatewayMACAddress in gatewayMACAddresses)
        {
            if (string.Equals(macAddress, gatewayMACAddress, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}

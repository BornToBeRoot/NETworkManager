using System.Collections.Generic;
using NETworkManager.Utilities;

namespace NETworkManager.Models.Network;

/// <summary>
///     Class containing information about a host in a <see cref="IPScanner" />. Created by the scanner when a host is
///     found and updated afterwards, as further results arrive (see <see cref="IPScanner.HostUpdated" />). Values are
///     only changed through the updates of the scanner, which the consumer applies on the UI thread.
/// </summary>
public class IPScannerHostInfo : PropertyChangedBase
{
    /// <summary>
    ///     Creates a new instance of <see cref="IPScannerHostInfo" /> with the results of the discovery.
    /// </summary>
    /// <param name="isReachable">Indicates whether the host is reachable.</param>
    /// <param name="pingInfo">Information about the ping to the host.</param>
    /// <param name="isAnyPortOpen">Indicates whether any port is open.</param>
    /// <param name="ports">List of ports to show (open ports, all ports if unreachable ones are shown).</param>
    /// <param name="netBIOSInfo">NetBIOS information about the host.</param>
    public IPScannerHostInfo(bool isReachable, PingInfo pingInfo, bool isAnyPortOpen, List<PortInfo> ports,
        NetBIOSInfo netBIOSInfo)
    {
        IsReachable = isReachable;
        PingInfo = pingInfo;
        IsAnyPortOpen = isAnyPortOpen;
        Ports = ports;
        NetBIOSInfo = netBIOSInfo;
    }

    /// <summary>
    ///     Indicates whether the host is reachable.
    /// </summary>
    public bool IsReachable
    {
        get;
        internal set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Information about the ping to the host.
    /// </summary>
    public PingInfo PingInfo
    {
        get;
        internal set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Hostname of the host: DNS hostname, fallback to the NetBIOS computer name.
    /// </summary>
    public string Hostname => !string.IsNullOrEmpty(DNSHostname)
        ? DNSHostname
        : NetBIOSInfo?.ComputerName ?? string.Empty;

    /// <summary>
    ///     DNS hostname of the host (PTR record), empty if not resolved (yet).
    /// </summary>
    public string DNSHostname
    {
        get;
        internal set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Hostname));
        }
    } = string.Empty;

    /// <summary>
    ///     Indicates whether any port is open.
    /// </summary>
    public bool IsAnyPortOpen
    {
        get;
        internal set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     List of ports to show (open ports, all ports if unreachable ones are shown). Replaced, never changed.
    /// </summary>
    public List<PortInfo> Ports
    {
        get;
        internal set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     NetBIOS information about the host.
    /// </summary>
    public NetBIOSInfo NetBIOSInfo
    {
        get;
        internal set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Hostname));
            OnPropertyChanged(nameof(MACAddress));
            OnPropertyChanged(nameof(Vendor));
        }
    }

    /// <summary>
    ///     MAC address from the neighbor table (ARP / NDP) or of a local network interface, empty if not resolved
    ///     (yet).
    /// </summary>
    public string NeighborMACAddress
    {
        get;
        internal set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(MACAddress));
            OnPropertyChanged(nameof(Vendor));
        }
    } = string.Empty;

    /// <summary>
    ///     Vendor of <see cref="NeighborMACAddress" />.
    /// </summary>
    public string NeighborVendor
    {
        get;
        internal set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Vendor));
        }
    } = string.Empty;

    /// <summary>
    ///     MAC address of the host: neighbor table (ARP / NDP) is preferred, fallback to NetBIOS.
    /// </summary>
    public string MACAddress => !string.IsNullOrEmpty(NeighborMACAddress)
        ? NeighborMACAddress
        : NetBIOSInfo?.MACAddress ?? string.Empty;

    /// <summary>
    ///     Vendor of the host based on <see cref="MACAddress" />.
    /// </summary>
    public string Vendor => !string.IsNullOrEmpty(NeighborMACAddress)
        ? NeighborVendor
        : NetBIOSInfo?.Vendor ?? string.Empty;

    /// <summary>
    ///     Creates a copy with the current values, e.g. for an export in the background while the scan still updates
    ///     the host. Call it on the thread that applies the updates (UI thread).
    /// </summary>
    /// <returns>Copy of the host information (without event subscribers).</returns>
    public IPScannerHostInfo Clone()
    {
        return new IPScannerHostInfo(IsReachable, PingInfo, IsAnyPortOpen, Ports, NetBIOSInfo)
        {
            DNSHostname = DNSHostname,
            NeighborMACAddress = NeighborMACAddress,
            NeighborVendor = NeighborVendor
        };
    }
}

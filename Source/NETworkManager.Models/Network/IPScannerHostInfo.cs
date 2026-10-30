using System.Collections.Generic;
using System.Net;
using NETworkManager.Utilities;

namespace NETworkManager.Models.Network;

/// <summary>
///     Class containing information about a host in a <see cref="IPScanner" />. Created by the scanner when a host
///     answered the first probe and updated afterwards, as further results arrive (see
///     <see cref="IPScanner.HostUpdated" />): values of probes that are still running are placeholders, marked by the
///     <c>Is...Pending</c> properties. Values are only changed through the updates of the scanner, which the consumer
///     applies on the UI thread.
/// </summary>
public class IPScannerHostInfo : PropertyChangedBase
{
    /// <summary>
    ///     Creates a new instance of <see cref="IPScannerHostInfo" /> with the results of the discovery.
    /// </summary>
    /// <param name="ipAddress">Scanned IP address.</param>
    /// <param name="isReachable">Indicates whether the host is reachable.</param>
    /// <param name="pingInfo">Information about the ping to the host.</param>
    /// <param name="isAnyPortOpen">Indicates whether any port is open.</param>
    /// <param name="ports">List of ports to show (open ports, all ports if unreachable ones are shown).</param>
    /// <param name="netBIOSInfo">NetBIOS information about the host.</param>
    public IPScannerHostInfo(IPAddress ipAddress, bool isReachable, PingInfo pingInfo, bool isAnyPortOpen,
        List<PortInfo> ports, NetBIOSInfo netBIOSInfo)
    {
        IPAddress = ipAddress;
        IsReachable = isReachable;
        PingInfo = pingInfo;
        IsAnyPortOpen = isAnyPortOpen;
        Ports = ports;
        NetBIOSInfo = netBIOSInfo;
    }

    /// <summary>
    ///     Scanned IP address (input of the scan, e.g. with the IPv6 scope id). <see cref="PingInfo" /> contains the
    ///     address of the ping reply.
    /// </summary>
    public IPAddress IPAddress { get; }

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
    ///     Probes the host answered so far, i.e. how it was detected as reachable.
    ///     <see cref="IPScannerDetectionMethod.None" /> if it is unreachable.
    /// </summary>
    public IPScannerDetectionMethod DetectedBy
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
    ///     Indicates whether the ping is still running (<see cref="PingInfo" /> is a placeholder).
    /// </summary>
    public bool IsPingPending
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
    ///     Indicates whether the port scan is still running (<see cref="Ports" /> and <see cref="IsAnyPortOpen" />
    ///     are placeholders).
    /// </summary>
    public bool IsPortScanPending
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
    ///     Indicates whether the NetBIOS request is still running (<see cref="NetBIOSInfo" /> is a placeholder).
    /// </summary>
    public bool IsNetBIOSPending
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
    ///     Takes over all values of another result of the same host (e.g. a rescan), so the shown row keeps its
    ///     identity (selection, expanded details). Call it on the thread that owns the host (UI thread).
    /// </summary>
    /// <param name="source">New result of the host.</param>
    public void CopyFrom(IPScannerHostInfo source)
    {
        IsReachable = source.IsReachable;
        DetectedBy = source.DetectedBy;
        PingInfo = source.PingInfo;
        IsAnyPortOpen = source.IsAnyPortOpen;
        Ports = source.Ports;
        NetBIOSInfo = source.NetBIOSInfo;
        IsPingPending = source.IsPingPending;
        IsPortScanPending = source.IsPortScanPending;
        IsNetBIOSPending = source.IsNetBIOSPending;
        DNSHostname = source.DNSHostname;
        NeighborMACAddress = source.NeighborMACAddress;
        NeighborVendor = source.NeighborVendor;
    }

    /// <summary>
    ///     Creates a copy with the current values, e.g. for an export in the background while the scan still updates
    ///     the host. Call it on the thread that applies the updates (UI thread).
    /// </summary>
    /// <returns>Copy of the host information (without event subscribers).</returns>
    public IPScannerHostInfo Clone()
    {
        return new IPScannerHostInfo(IPAddress, IsReachable, PingInfo, IsAnyPortOpen, Ports, NetBIOSInfo)
        {
            DetectedBy = DetectedBy,
            IsPingPending = IsPingPending,
            IsPortScanPending = IsPortScanPending,
            IsNetBIOSPending = IsNetBIOSPending,
            DNSHostname = DNSHostname,
            NeighborMACAddress = NeighborMACAddress,
            NeighborVendor = NeighborVendor
        };
    }
}

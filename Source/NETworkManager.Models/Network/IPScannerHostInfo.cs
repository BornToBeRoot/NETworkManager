using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
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
    ///     Roles of the host for the local computer (e.g. its default gateway), see
    ///     <see cref="IPScannerHostRoleHelper" />.
    /// </summary>
    public IPScannerHostRole Role
    {
        get;
        internal set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PrimaryRole));
        }
    }

    /// <summary>
    ///     Most important role of the host (for its icon), see <see cref="IPScannerHostRoleHelper.GetPrimaryRole" />.
    /// </summary>
    public IPScannerHostRole PrimaryRole => IPScannerHostRoleHelper.GetPrimaryRole(Role);

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
            OnPropertyChanged(nameof(OSGuess));
            OnPropertyChanged(nameof(OSGuessHops));
        }
    }

    /// <summary>
    ///     Operating system guessed from the TTL of the ping reply (see <see cref="IPScannerOSGuessHelper" />),
    ///     <see cref="IPScannerOSGuess.None" /> without a ping reply (or TTL, e.g. IPv6).
    /// </summary>
    public IPScannerOSGuess OSGuess => GetOSGuess().osGuess;

    /// <summary>
    ///     Number of hops to the host based on the TTL of the ping reply (see <see cref="OSGuess" />).
    /// </summary>
    public int OSGuessHops => GetOSGuess().hops;

    private (IPScannerOSGuess osGuess, int initialTTL, int hops) GetOSGuess()
    {
        return !IsPingPending && PingInfo?.Status == IPStatus.Success
            ? IPScannerOSGuessHelper.GetFromTTL(PingInfo.TTL)
            : (IPScannerOSGuess.None, 0, 0);
    }

    /// <summary>
    ///     Hostname of the host: DNS hostname, fallback to the mDNS hostname and the NetBIOS computer name.
    /// </summary>
    public string Hostname => !string.IsNullOrEmpty(DNSHostname)
        ? DNSHostname
        : !string.IsNullOrEmpty(MDNSHostname)
            ? MDNSHostname
            : NetBIOSInfo?.ComputerName ?? string.Empty;

    /// <summary>
    ///     mDNS hostname of the host (e.g. "macbook.local", see <see cref="MDNSResolver" />), empty if not resolved
    ///     (yet).
    /// </summary>
    public string MDNSHostname
    {
        get;
        internal set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsHostnamePending));
            OnPropertyChanged(nameof(Hostname));
        }
    } = string.Empty;

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
            OnPropertyChanged(nameof(IsHostnamePending));
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
            OnPropertyChanged(nameof(OSGuess));
            OnPropertyChanged(nameof(OSGuessHops));
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
            OnPropertyChanged(nameof(IsHostnamePending));
            OnPropertyChanged(nameof(IsMACAddressPending));
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
            OnPropertyChanged(nameof(IsMACAddressPending));
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
    ///     Indicates whether <see cref="Hostname" /> may still change: the DNS hostname is resolved, or there is none
    ///     (yet) and the mDNS hostname or - without both - the NetBIOS name is still requested.
    /// </summary>
    public bool IsHostnamePending => IsDNSPending ||
                                     (string.IsNullOrEmpty(DNSHostname) &&
                                      (IsMDNSPending || (string.IsNullOrEmpty(MDNSHostname) && IsNetBIOSPending)));

    /// <summary>
    ///     Indicates whether <see cref="MACAddress" /> and <see cref="Vendor" /> may still change: the MAC address is
    ///     resolved from the neighbor table, or there is none and the NetBIOS answer (fallback) is still requested.
    /// </summary>
    public bool IsMACAddressPending => IsNeighborMACAddressPending ||
                                       (string.IsNullOrEmpty(NeighborMACAddress) && IsNetBIOSPending);

    /// <summary>
    ///     Title of the web page of the host, see <see cref="WebPageResolver" />. Empty if it has none or it was not
    ///     read (yet).
    /// </summary>
    public string WebTitle
    {
        get;
        internal set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(WebSummary));
        }
    } = string.Empty;

    /// <summary>
    ///     Short summary of the web page for the result: the title, the <c>Server</c> header if the page has no title.
    /// </summary>
    public string WebSummary => !string.IsNullOrEmpty(WebTitle) ? WebTitle : WebServer;

    /// <summary>
    ///     HTTP status code of the web page (e.g. 200), 0 if none answered (yet).
    /// </summary>
    public int WebStatusCode
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
    ///     Value of the <c>Server</c> header of the web page, empty if there is none.
    /// </summary>
    public string WebServer
    {
        get;
        internal set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(WebSummary));
        }
    } = string.Empty;

    /// <summary>
    ///     Value of the <c>X-Powered-By</c> header of the web page, empty if there is none.
    /// </summary>
    public string WebPoweredBy
    {
        get;
        internal set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    /// <summary>
    ///     URL of the web page that answered (e.g. "https://192.168.1.1/"), empty if none answered (yet).
    /// </summary>
    public string WebURL
    {
        get;
        internal set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    /// <summary>
    ///     Name of the host read via SNMP (sysName), empty if not read (yet).
    /// </summary>
    public string SNMPName
    {
        get;
        internal set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    /// <summary>
    ///     Description of the host read via SNMP (sysDescr), empty if not read (yet).
    /// </summary>
    public string SNMPDescription
    {
        get;
        internal set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    /// <summary>
    ///     Location of the host read via SNMP (sysLocation), empty if not read (yet).
    /// </summary>
    public string SNMPLocation
    {
        get;
        internal set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    /// <summary>
    ///     Indicates whether the DNS hostname (PTR) is still resolved (<see cref="DNSHostname" /> is a placeholder).
    /// </summary>
    public bool IsDNSPending
    {
        get;
        internal set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsHostnamePending));
        }
    }

    /// <summary>
    ///     Indicates whether the MAC address is still resolved from the neighbor table (<see cref="NeighborMACAddress" />
    ///     and <see cref="NeighborVendor" /> are placeholders).
    /// </summary>
    public bool IsNeighborMACAddressPending
    {
        get;
        internal set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsMACAddressPending));
        }
    }

    /// <summary>
    ///     Indicates whether the mDNS request is still running (<see cref="MDNSHostname" /> is a placeholder).
    /// </summary>
    public bool IsMDNSPending
    {
        get;
        internal set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsHostnamePending));
        }
    }

    /// <summary>
    ///     Indicates whether the title of the web page is still read, or the port scan it needs is still running
    ///     (<see cref="WebTitle" /> is a placeholder).
    /// </summary>
    public bool IsWebPagePending
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
    ///     Indicates whether the SNMP request is still running (<see cref="SNMPName" />,
    ///     <see cref="SNMPDescription" />, <see cref="SNMPLocation" /> are placeholders).
    /// </summary>
    public bool IsSNMPPending
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
    ///     Takes over all values of another result of the same host (e.g. a rescan), so the shown row keeps its
    ///     identity (selection, expanded details). Call it on the thread that owns the host (UI thread).
    /// </summary>
    /// <param name="source">New result of the host.</param>
    public void CopyFrom(IPScannerHostInfo source)
    {
        IsReachable = source.IsReachable;
        DetectedBy = source.DetectedBy;
        Role = source.Role;
        PingInfo = source.PingInfo;
        IsAnyPortOpen = source.IsAnyPortOpen;
        Ports = source.Ports;
        NetBIOSInfo = source.NetBIOSInfo;
        IsPingPending = source.IsPingPending;
        IsPortScanPending = source.IsPortScanPending;
        IsNetBIOSPending = source.IsNetBIOSPending;
        DNSHostname = source.DNSHostname;
        MDNSHostname = source.MDNSHostname;
        WebTitle = source.WebTitle;
        WebURL = source.WebURL;
        WebStatusCode = source.WebStatusCode;
        WebServer = source.WebServer;
        WebPoweredBy = source.WebPoweredBy;
        SNMPName = source.SNMPName;
        SNMPDescription = source.SNMPDescription;
        SNMPLocation = source.SNMPLocation;
        IsDNSPending = source.IsDNSPending;
        IsNeighborMACAddressPending = source.IsNeighborMACAddressPending;
        IsMDNSPending = source.IsMDNSPending;
        IsWebPagePending = source.IsWebPagePending;
        IsSNMPPending = source.IsSNMPPending;
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
            Role = Role,
            IsPingPending = IsPingPending,
            IsPortScanPending = IsPortScanPending,
            IsNetBIOSPending = IsNetBIOSPending,
            DNSHostname = DNSHostname,
            MDNSHostname = MDNSHostname,
            WebTitle = WebTitle,
            WebURL = WebURL,
            WebStatusCode = WebStatusCode,
            WebServer = WebServer,
            WebPoweredBy = WebPoweredBy,
            SNMPName = SNMPName,
            SNMPDescription = SNMPDescription,
            SNMPLocation = SNMPLocation,
            IsDNSPending = IsDNSPending,
            IsNeighborMACAddressPending = IsNeighborMACAddressPending,
            IsMDNSPending = IsMDNSPending,
            IsWebPagePending = IsWebPagePending,
            IsSNMPPending = IsSNMPPending,
            NeighborMACAddress = NeighborMACAddress,
            NeighborVendor = NeighborVendor
        };
    }
}

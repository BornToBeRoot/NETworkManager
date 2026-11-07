namespace NETworkManager.Models.Network;

/// <summary>
///     Class contains the options for the IP scanner. A record, so a copy with other values can be created with
///     <c>with</c> (e.g. for a rescan).
/// </summary>
public record IPScannerOptions
{
    /// <summary>
    ///     Max concurrent threads for the host scan.
    /// </summary>
    public required int MaxHostThreads { get; init; }

    /// <summary>
    ///     Max concurrent threads for the port scan of each host.
    /// </summary>
    public required int MaxPortThreads { get; init; }

    /// <summary>
    ///     Ping the host.
    /// </summary>
    public required bool ICMPEnabled { get; init; }

    /// <summary>
    ///     Number of attempts to ping a host.
    /// </summary>
    public required int ICMPAttempts { get; init; }

    /// <summary>
    ///     Timeout in milliseconds after which a ping is considered lost.
    /// </summary>
    public required int ICMPTimeout { get; init; }

    /// <summary>
    ///     Size of the buffer used in the ping in bytes. A size, not the buffer, so options with the same values are
    ///     equal (records compare arrays by reference).
    /// </summary>
    public required int ICMPBufferSize { get; init; }

    /// <summary>
    ///     Guess the operating system from the TTL of the ping reply (only shown in the result, no extra traffic).
    /// </summary>
    public required bool OSGuessEnabled { get; init; }

    /// <summary>
    ///     Resolve the hostname for an IP address.
    /// </summary>
    public required bool ResolveHostname { get; init; }

    /// <summary>
    ///     Scan the ports of the host.
    /// </summary>
    public required bool PortScanEnabled { get; init; }

    /// <summary>
    ///     Ports to scan.
    /// </summary>
    public required int[] PortScanPorts { get; init; }

    /// <summary>
    ///     Timeout in milliseconds after which a port is considered closed.
    /// </summary>
    public required int PortScanTimeout { get; init; }

    /// <summary>
    ///     Number of attempts to connect to a port if there was no answer (timed out).
    /// </summary>
    public required int PortScanAttempts { get; init; }

    /// <summary>
    ///     Resolve the NetBIOS name for an IP address.
    /// </summary>
    public required bool NetBIOSEnabled { get; init; }

    /// <summary>
    ///     Timeout in milliseconds after which a NetBIOS request is considered lost.
    /// </summary>
    public required int NetBIOSTimeout { get; init; }

    /// <summary>
    ///     Find hosts in a local subnet that didn't answer any probe by their neighbor entry (ARP / NDP), see
    ///     <see cref="IPScannerNeighborCheck" />.
    /// </summary>
    public required bool ARPEnabled { get; init; }

    /// <summary>
    ///     Resolve the MAC address for the host from ARP.
    /// </summary>
    public required bool ResolveMACAddress { get; init; }

    /// <summary>
    ///     Resolve the mDNS hostname of reachable hosts, see <see cref="MDNSResolver" />.
    /// </summary>
    public required bool MDNSEnabled { get; init; }

    /// <summary>
    ///     Timeout in milliseconds after which an mDNS request is considered lost.
    /// </summary>
    public required int MDNSTimeout { get; init; }

    /// <summary>
    ///     Read the title of the web page of reachable hosts with an open web port, see
    ///     <see cref="WebPageResolver" />.
    /// </summary>
    public required bool WebPageEnabled { get; init; }

    /// <summary>
    ///     Ports whose web page title is read, in this order (only open ports, the first that answers is used).
    /// </summary>
    public required int[] WebPagePorts { get; init; }

    /// <summary>
    ///     Timeout in milliseconds of a web request.
    /// </summary>
    public required int WebPageTimeout { get; init; }

    /// <summary>
    ///     Read the SNMP system information of reachable hosts, see <see cref="SNMPSystemInfoResolver" />.
    /// </summary>
    public required bool SNMPEnabled { get; init; }

    /// <summary>
    ///     SNMP version and credentials (community for v1 / v2c, user for v3): of the settings, or the override of the
    ///     profile / group the tab was opened with.
    /// </summary>
    public required SNMPCredentials SNMPCredentials { get; init; }

    /// <summary>
    ///     Timeout in milliseconds after which an SNMP request is considered lost.
    /// </summary>
    public required int SNMPTimeout { get; init; }

    /// <summary>
    ///     Read the Windows host information of reachable hosts (SMB / NTLM without credentials, NetApi with the
    ///     credentials below), see <see cref="SMBInfoResolver" /> and <see cref="WindowsNetApiResolver" />.
    /// </summary>
    public required bool WindowsEnabled { get; init; }

    /// <summary>
    ///     Windows credentials of the profile / group the tab was opened with for the NetApi requests, null for the user
    ///     running the application (there are no Windows credentials in the settings).
    /// </summary>
    public WindowsCredentials WindowsCredentials { get; init; }

    /// <summary>
    ///     Lockout protection of the <see cref="WindowsCredentials" />, kept by the tab for all its scans (accepted /
    ///     rejected state, a check that is still running; the logon failures are counted per account for all tabs), so
    ///     repeated scans don't lock the account. Null: a new one for the scan.
    /// </summary>
    public WindowsLogonGate WindowsLogonGate { get; init; }

    /// <summary>
    ///     Timeout in milliseconds of the SMB request and of the NetApi requests of a host.
    /// </summary>
    public required int WindowsTimeout { get; init; }

    /// <summary>
    ///     Max. logon failures with the <see cref="WindowsCredentials" /> on hosts of other domains before they are
    ///     rejected, see <see cref="WindowsLogonGate" />.
    /// </summary>
    public required int WindowsMaxLogonFailures { get; init; }

    /// <summary>
    ///     Include unreachable IP addresses in the result.
    /// </summary>
    public required bool ShowAllResults { get; init; }

    /// <summary>
    ///     Raise every host in <see cref="IPScanner.HostFound" />, also unreachable ones (e.g. for a rescan of shown
    ///     hosts), without showing all ports like <see cref="ShowAllResults" />.
    /// </summary>
    public required bool ReportAllHosts { get; init; }

    /// <summary>
    ///     Hosts that are raised in <see cref="IPScanner.HostFound" /> also if they are unreachable (e.g. the shown rows
    ///     of a repeated scan, so a host that is down now updates its row) - unlike <see cref="ReportAllHosts" />, the
    ///     other unreachable hosts are not created and looked up. Null for none.
    /// </summary>
    public System.Collections.Generic.IReadOnlySet<System.Net.IPAddress> ReportHosts { get; init; }
}

namespace NETworkManager.Models.Network;

/// <summary>
///     Options of an IP scan that decide which columns and details of its result are shown. Only these flags of the
///     <see cref="IPScannerOptions" />, without the ports and the ICMP buffer of the scan.
/// </summary>
/// <param name="ICMPEnabled">The hosts were pinged.</param>
/// <param name="OSGuessEnabled">The operating system was guessed from the TTL of the ping reply.</param>
/// <param name="ResolveHostname">The hostname was resolved via DNS.</param>
/// <param name="PortScanEnabled">The ports of the hosts were scanned.</param>
/// <param name="NetBIOSEnabled">The NetBIOS information was requested.</param>
/// <param name="ResolveMACAddress">The MAC address and vendor were resolved.</param>
/// <param name="MDNSEnabled">The mDNS hostname was resolved.</param>
/// <param name="WebPageEnabled">The title of the web pages was read.</param>
/// <param name="SNMPEnabled">The SNMP system information was read.</param>
public sealed record IPScannerResultOptions(
    bool ICMPEnabled,
    bool OSGuessEnabled,
    bool ResolveHostname,
    bool PortScanEnabled,
    bool NetBIOSEnabled,
    bool ResolveMACAddress,
    bool MDNSEnabled,
    bool WebPageEnabled,
    bool SNMPEnabled)
{
    /// <summary>
    ///     Creates the result options from the options of a scan.
    /// </summary>
    /// <param name="options">Options of the scan.</param>
    public IPScannerResultOptions(IPScannerOptions options)
        : this(options.ICMPEnabled, options.OSGuessEnabled, options.ResolveHostname, options.PortScanEnabled, options.NetBIOSEnabled, options.ResolveMACAddress,
            options.MDNSEnabled, options.WebPageEnabled, options.SNMPEnabled)
    {
    }
}

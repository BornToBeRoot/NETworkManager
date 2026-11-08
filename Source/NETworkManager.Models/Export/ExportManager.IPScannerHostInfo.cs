using System;
using System.Collections.Generic;
using System.Linq;
using NETworkManager.Models.Network;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    ///     Method to export objects from type <see cref="IPScannerHostInfo" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{HostInfo}" /> to export.</param>
    /// <param name="options">
    ///     Options of the scan the hosts are from. Values of disabled probes are exported as empty values instead of
    ///     their placeholders (e.g. port status "Closed" if the port scan was disabled).
    /// </param>
    public static void Export(string filePath, ExportFileType fileType,
        IReadOnlyList<IPScannerHostInfo> collection, IPScannerResultOptions options)
    {
        // Skip if critical data is null
        Export(filePath, fileType, collection.Where(info => info?.PingInfo != null),
            GetIPScannerHostInfoExportFields(options), ApplicationName.IPScanner);
    }

    /// <summary>
    ///     Exported fields of a <see cref="PortInfo" /> (ports of a host, see <see cref="ExportList{TItem}" />).
    /// </summary>
    private static readonly ExportField<PortInfo>[] PortInfoExportFields =
    [
        new(nameof(PortInfo.Port), x => x.Port),
        new(nameof(PortInfo.LookupInfo.Protocol), x => x.LookupInfo.Protocol),
        new(nameof(PortInfo.LookupInfo.Service), x => x.LookupInfo.Service),
        new(nameof(PortInfo.LookupInfo.Description), x => x.LookupInfo.Description),
        new(nameof(PortInfo.State), x => x.State)
    ];

    /// <summary>
    ///     Exported fields of a <see cref="PortInfo" /> in the CSV file: without service and description (free text, may
    ///     contain <c>/</c> or <c>;</c>), so the value can be split again (e.g. <c>80/Tcp/Open; 443/Tcp/Closed</c>).
    /// </summary>
    private static readonly ExportField<PortInfo>[] PortInfoCsvExportFields =
    [
        PortInfoExportFields[0], PortInfoExportFields[1], PortInfoExportFields[4]
    ];

    /// <summary>
    ///     Gets the exported fields of an <see cref="IPScannerHostInfo" />, grouped by topic (host, ping, ports, NetBIOS,
    ///     web page, SNMP, Windows, change since the previous scan). Values of a probe that is still running
    ///     (export during a scan, or a scan canceled before the probe finished) or disabled are placeholders - exported
    ///     as empty values instead of a result. The ports are an <see cref="ExportList{TItem}" /> (port / protocol /
    ///     state joined in the CSV file, one element per port in XML, an array in JSON).
    /// </summary>
    /// <param name="options">Options of the scan the hosts are from.</param>
    /// <returns>Fields in the order of the columns.</returns>
    private static ExportField<IPScannerHostInfo>[] GetIPScannerHostInfoExportFields(IPScannerResultOptions options)
    {
        var isWebPageRead = IsWebPageRead(options);

        Func<IPScannerHostInfo, object> IfEnabled(bool isEnabled, Func<IPScannerHostInfo, object> value) =>
            info => isEnabled ? value(info) : null;

        // Ping values only with a reply (time, TTL, bytes)
        Func<IPScannerHostInfo, object> IfPingReply(Func<PingInfo, object> value) => info =>
            GetPingInfo(info, options) is { } ping && HasPingReply(ping.Status) ? value(ping) : null;

        Func<IPScannerHostInfo, object> IfNetBIOS(Func<NetBIOSInfo, object> value) => info =>
            GetNetBIOSInfo(info, options) is { } netBIOSInfo ? value(netBIOSInfo) : null;

        Func<IPScannerHostInfo, object> IfCertificate(Func<WebCertificateInfo, object> value) => info =>
            GetWebCertificate(info, options) is { } certificate ? value(certificate) : null;

        // Windows host information: null (empty values) if disabled, "" if the host had none
        Func<IPScannerHostInfo, object> IfWindows(Func<WindowsHostInfo, string> value) => info =>
            options.WindowsEnabled ? info.Windows == null ? string.Empty : value(info.Windows) ?? string.Empty : null;

        // Number / bool like WebStatusCode / WebCertificateSelfSigned: null without a value
        Func<IPScannerHostInfo, object> IfWindowsValue(Func<WindowsHostInfo, object> value) => info =>
            options.WindowsEnabled && info.Windows != null ? value(info.Windows) : null;

        return
        [
            // Host (the hostname is the DNS name, or the mDNS / NetBIOS name as fallback)
            new(nameof(IPScannerHostInfo.IsReachable), x => x.IsReachable),
            new(nameof(IPScannerHostInfo.DetectedBy), GetDetectedBy, true),
            new(nameof(PingInfo.IPAddress), x => x.PingInfo.IPAddress),
            new(nameof(IPScannerHostInfo.Hostname), x => x.Hostname, true),
            new(nameof(IPScannerHostInfo.DNSHostname), x => x.DNSHostname, true),
            new(nameof(IPScannerHostInfo.MDNSHostname), IfEnabled(options.MDNSEnabled, x => x.MDNSHostname), true),
            new(nameof(IPScannerHostInfo.MACAddress), x => x.MACAddress),
            new(nameof(IPScannerHostInfo.Vendor), x => x.Vendor, true),
            new(nameof(IPScannerHostInfo.Role), GetRole, true),
            new(nameof(IPScannerHostInfo.OSGuess), x => GetOSGuess(x, options)),
            // Ping (time in ms, time / TTL / bytes only with a reply)
            new("PingStatus", x => GetPingInfo(x, options)?.Status),
            new(nameof(PingInfo.Timestamp), x => GetPingInfo(x, options)?.Timestamp),
            new(nameof(PingInfo.Time), IfPingReply(x => x.Time)),
            new(nameof(PingInfo.TTL), IfPingReply(x => x.TTL)),
            new(nameof(PingInfo.Bytes), IfPingReply(x => x.Bytes)),
            // Port scan
            new("PortStatus", x => GetPortStatus(x, options)),
            new(nameof(IPScannerHostInfo.Ports), x => new ExportList<PortInfo>(nameof(PortInfo),
                x.Ports != null && options.PortScanEnabled ? x.Ports : [], PortInfoExportFields,
                PortInfoCsvExportFields), true),
            // NetBIOS
            new("NetBIOSIsReachable", IfNetBIOS(x => x.IsReachable)),
            new("NetBIOSIPAddress", IfNetBIOS(x => x.IPAddress)),
            new("NetBIOSComputerName", IfNetBIOS(x => x.ComputerName), true),
            new("NetBIOSUserName", IfNetBIOS(x => x.UserName), true),
            new("NetBIOSGroupName", IfNetBIOS(x => x.GroupName), true),
            new("NetBIOSMACAddress", IfNetBIOS(x => x.MACAddress)),
            new("NetBIOSVendor", IfNetBIOS(x => x.Vendor), true),
            // Web page (only read with the port scan)
            new(nameof(IPScannerHostInfo.WebURL), IfEnabled(isWebPageRead, x => x.WebURL), true),
            new(nameof(IPScannerHostInfo.WebStatusCode), x => GetWebStatusCode(x, options)),
            new(nameof(IPScannerHostInfo.WebTitle), IfEnabled(isWebPageRead, x => x.WebTitle), true),
            new(nameof(IPScannerHostInfo.WebServer), IfEnabled(isWebPageRead, x => x.WebServer), true),
            new(nameof(IPScannerHostInfo.WebPoweredBy), IfEnabled(isWebPageRead, x => x.WebPoweredBy), true),
            new(nameof(IPScannerHostInfo.WebRealm), IfEnabled(isWebPageRead, x => x.WebRealm), true),
            new(nameof(IPScannerHostInfo.WebRedirectURL), IfEnabled(isWebPageRead, x => x.WebRedirectURL), true),
            new("WebCertificateSubject", IfCertificate(x => x.Subject), true),
            new("WebCertificateIssuer", IfCertificate(x => x.Issuer), true),
            new("WebCertificateNotAfter", IfCertificate(x => x.NotAfter.ToLocalTime())),
            new("WebCertificateSelfSigned", IfCertificate(x => x.IsSelfSigned)),
            // SNMP system information (SNMPError: SNMPv3 error of the host, e.g. "AuthenticationFailed")
            new(nameof(IPScannerHostInfo.SNMPName), IfEnabled(options.SNMPEnabled, x => x.SNMPName), true),
            new(nameof(IPScannerHostInfo.SNMPDescription), IfEnabled(options.SNMPEnabled, x => x.SNMPDescription),
                true),
            new(nameof(IPScannerHostInfo.SNMPLocation), IfEnabled(options.SNMPEnabled, x => x.SNMPLocation), true),
            new(nameof(IPScannerHostInfo.SNMPError),
                IfEnabled(options.SNMPEnabled, x => x.HasSNMPError ? x.SNMPError.ToString() : string.Empty), true),
            // Windows host information (SMB / NTLM, NetApi). WindowsTimeOffset: seconds (time of the host minus the local
            // time), WindowsError: Win32 error of the NetApi requests (e.g. 5 = access denied, 1326 = wrong username or
            // password)
            new("WindowsOS", IfWindows(x => x.OS), true),
            new("WindowsEdition", IfWindows(x => x.Edition), true),
            new("WindowsVersion", IfWindows(x => x.Version?.ToString())),
            new("WindowsRole", IfWindows(x => x.Role == WindowsHostRole.Unknown ? string.Empty : x.Role.ToString())),
            new("WindowsComputerName", IfWindows(x => x.ComputerName), true),
            new("WindowsDomain", IfWindows(x => x.Domain), true),
            new("WindowsTimeOffset",
                IfWindowsValue(x => x.TimeOffset is { } offset ? Math.Round(offset.TotalSeconds, 3) : null)),
            new("WindowsUsers", IfWindows(x => x.Users == null ? string.Empty : string.Join(ListSeparator, x.Users)),
                true),
            new("WindowsDescription", IfWindows(x => x.Description), true),
            new("WindowsError", IfWindowsValue(x => x.Error == 0 ? null : x.Error)),
            new("SMBDialect", IfWindows(x => x.SMB?.Dialect)),
            new("SMBSigningRequired", IfWindowsValue(x => x.SMB?.SigningRequired)),
            new("SMBServerGuid", IfWindows(x => x.SMB?.ServerGuid.ToString())),
            // Change since the previous scan of the same input (empty without a previous scan / change)
            new(nameof(IPScannerHostInfo.Change), x => x.Change == IPScannerHostChange.None ? null : x.Change),
            new(nameof(IPScannerHostInfo.PreviousMACAddress), x => x.PreviousMACAddress)
        ];
    }

    /// <summary>
    ///     Gets the probes a host answered (e.g. "ICMP, TCP"), null if it is unreachable.
    /// </summary>
    /// <param name="info">Host to get the probes for.</param>
    /// <returns>Probes or null.</returns>
    private static string GetDetectedBy(IPScannerHostInfo info)
    {
        return info.DetectedBy == IPScannerDetectionMethod.None ? null : info.DetectedBy.ToString();
    }

    /// <summary>
    ///     Checks if the web pages were read in the scan: only with the port scan (only open ports are requested).
    /// </summary>
    /// <param name="options">Options of the scan the hosts are from.</param>
    /// <returns>True if the web pages were read.</returns>
    private static bool IsWebPageRead(IPScannerResultOptions options)
    {
        return options.PortScanEnabled && options.WebPageEnabled;
    }

    /// <summary>
    ///     Gets the HTTP status code of the web page of a host, null if none answered or the web page was not read.
    /// </summary>
    /// <param name="info">Host to get the status code for.</param>
    /// <param name="options">Options of the scan the host is from.</param>
    /// <returns>Status code or null.</returns>
    private static int? GetWebStatusCode(IPScannerHostInfo info, IPScannerResultOptions options)
    {
        return IsWebPageRead(options) && info.WebStatusCode > 0 ? info.WebStatusCode : null;
    }

    /// <summary>
    ///     Gets the TLS certificate of the web page of a host, null for http, if none answered or the web page was not
    ///     read.
    /// </summary>
    /// <param name="info">Host to get the certificate for.</param>
    /// <param name="options">Options of the scan the host is from.</param>
    /// <returns>Certificate or null.</returns>
    private static WebCertificateInfo GetWebCertificate(IPScannerHostInfo info, IPScannerResultOptions options)
    {
        return IsWebPageRead(options) ? info.WebCertificate : null;
    }

    /// <summary>
    ///     Gets the roles of a host for the local computer (e.g. "DefaultGateway, DNSServer"), null if it has none.
    /// </summary>
    /// <param name="info">Host to get the roles for.</param>
    /// <returns>Roles or null.</returns>
    private static string GetRole(IPScannerHostInfo info)
    {
        return info.Role == IPScannerHostRole.None ? null : info.Role.ToString();
    }

    /// <summary>
    ///     Gets the operating system guessed from the TTL (e.g. "Windows"), null if there is no guess or it was disabled.
    /// </summary>
    /// <param name="info">Host to get the guess for.</param>
    /// <param name="options">Options of the scan the host is from.</param>
    /// <returns>Guessed operating system or null.</returns>
    private static string GetOSGuess(IPScannerHostInfo info, IPScannerResultOptions options)
    {
        return options.ICMPEnabled && options.OSGuessEnabled && info.OSGuess != IPScannerOSGuess.None
            ? info.OSGuess.ToString()
            : null;
    }

    /// <summary>
    ///     Gets the result of the ping to a host - null while the ping is still running or if it was disabled
    ///     (placeholder).
    /// </summary>
    /// <param name="info">Host to get the ping result for.</param>
    /// <param name="options">Options of the scan the host is from.</param>
    /// <returns>Result of the ping or null.</returns>
    private static PingInfo GetPingInfo(IPScannerHostInfo info, IPScannerResultOptions options)
    {
        return info.IsPingPending || !options.ICMPEnabled ? null : info.PingInfo;
    }

    /// <summary>
    ///     Gets the NetBIOS information of a host - null while the NetBIOS request is still running or if it was
    ///     disabled (placeholder).
    /// </summary>
    /// <param name="info">Host to get the NetBIOS information for.</param>
    /// <param name="options">Options of the scan the host is from.</param>
    /// <returns>NetBIOS information or null.</returns>
    private static NetBIOSInfo GetNetBIOSInfo(IPScannerHostInfo info, IPScannerResultOptions options)
    {
        return info.IsNetBIOSPending || !options.NetBIOSEnabled ? null : info.NetBIOSInfo;
    }

    /// <summary>
    ///     Gets the port status of a host: open if any port is open, otherwise closed - null while the port scan is
    ///     still running (no result yet) or if it was disabled.
    /// </summary>
    /// <param name="info">Host to get the port status for.</param>
    /// <param name="options">Options of the scan the host is from.</param>
    /// <returns>Port status or null.</returns>
    private static PortState? GetPortStatus(IPScannerHostInfo info, IPScannerResultOptions options)
    {
        if (info.IsPortScanPending || !options.PortScanEnabled)
            return null;

        return info.IsAnyPortOpen ? PortState.Open : PortState.Closed;
    }
}
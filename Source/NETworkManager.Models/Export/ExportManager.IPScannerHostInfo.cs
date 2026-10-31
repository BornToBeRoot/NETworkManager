using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;
using Newtonsoft.Json;

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
        switch (fileType)
        {
            case ExportFileType.Csv:
                CreateCsv(collection, options, filePath);
                break;
            case ExportFileType.Xml:
                CreateXml(collection, options, filePath);
                break;
            case ExportFileType.Json:
                CreateJson(collection, options, filePath);
                break;
            case ExportFileType.Txt:
            default:
                throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
        }
    }

    /// <summary>
    ///     Value of an exported column (CSV) / element (XML, JSON key), see <see cref="GetFields" />.
    /// </summary>
    /// <param name="Name">Name of the column / element.</param>
    /// <param name="Value">Value (e.g. a bool, an enum or a string), null for an empty value.</param>
    /// <param name="IsQuoted">The value is quoted in the CSV file (text that may contain a comma).</param>
    private sealed record ExportField(string Name, object Value, bool IsQuoted = false);

    /// <summary>
    ///     Gets the exported values of a host - one list for CSV, XML and JSON. The released columns come first, new
    ///     ones are appended at the end, so the position of a column never changes. Values of a probe that is still
    ///     running (export during a scan, or a scan canceled before the probe finished) or disabled are placeholders -
    ///     exported as empty values instead of a result. The ports are a <see cref="List{PortInfo}" /> (joined in the CSV
    ///     file, one element per port in XML / JSON).
    /// </summary>
    /// <param name="info">Host to export.</param>
    /// <param name="options">Options of the scan the host is from.</param>
    /// <returns>Values in the order of the columns.</returns>
    private static List<ExportField> GetFields(IPScannerHostInfo info, IPScannerResultOptions options)
    {
        var ping = GetPingInfo(info, options);
        var netBIOSInfo = GetNetBIOSInfo(info, options);
        var certificate = GetWebCertificate(info, options);

        string IfEnabled(bool isEnabled, string value) => isEnabled ? value : null;

        return
        [
            new(nameof(IPScannerHostInfo.IsReachable), info.IsReachable),
            new(nameof(PingInfo.IPAddress), info.PingInfo.IPAddress),
            new(nameof(IPScannerHostInfo.Hostname), info.Hostname, true),
            new("PingStatus", ping?.Status),
            new(nameof(PingInfo.Timestamp),
                ping == null ? null : DateTimeHelper.DateTimeToFullDateTimeString(ping.Timestamp)),
            new(nameof(PingInfo.Time), ping == null ? null : Ping.TimeToString(ping.Status, ping.Time, true)),
            new(nameof(PingInfo.TTL), ping?.TTL),
            new(nameof(PingInfo.Bytes), ping?.Bytes),
            new("PortStatus", GetPortStatus(info, options)),
            new(nameof(IPScannerHostInfo.Ports), info.Ports != null && options.PortScanEnabled ? info.Ports : [], true),
            new("NetBIOSIsReachable", netBIOSInfo?.IsReachable),
            new("NetBIOSIPAddress", netBIOSInfo?.IPAddress),
            new("NetBIOSComputerName", netBIOSInfo?.ComputerName, true),
            new("NetBIOSUserName", netBIOSInfo?.UserName, true),
            new("NetBIOSGroupName", netBIOSInfo?.GroupName, true),
            new("NetBIOSMACAddress", netBIOSInfo?.MACAddress),
            new("NetBIOSVendor", netBIOSInfo?.Vendor, true),
            new(nameof(IPScannerHostInfo.MACAddress), info.MACAddress),
            new(nameof(IPScannerHostInfo.Vendor), info.Vendor, true),
            // Added later - after the released columns, so their position doesn't change
            new(nameof(IPScannerHostInfo.DNSHostname), info.DNSHostname, true),
            new(nameof(IPScannerHostInfo.DetectedBy), GetDetectedBy(info), true),
            new(nameof(IPScannerHostInfo.OSGuess), GetOSGuess(info, options)),
            new(nameof(IPScannerHostInfo.Role), GetRole(info), true),
            new(nameof(IPScannerHostInfo.MDNSHostname), IfEnabled(options.MDNSEnabled, info.MDNSHostname), true),
            new(nameof(IPScannerHostInfo.WebTitle), IfEnabled(options.WebPageEnabled, info.WebTitle), true),
            new(nameof(IPScannerHostInfo.WebURL), IfEnabled(options.WebPageEnabled, info.WebURL), true),
            new(nameof(IPScannerHostInfo.SNMPName), IfEnabled(options.SNMPEnabled, info.SNMPName), true),
            new(nameof(IPScannerHostInfo.SNMPDescription), IfEnabled(options.SNMPEnabled, info.SNMPDescription), true),
            new(nameof(IPScannerHostInfo.SNMPLocation), IfEnabled(options.SNMPEnabled, info.SNMPLocation), true),
            new(nameof(IPScannerHostInfo.WebStatusCode), GetWebStatusCode(info, options)),
            new(nameof(IPScannerHostInfo.WebServer), IfEnabled(options.WebPageEnabled, info.WebServer), true),
            new(nameof(IPScannerHostInfo.WebPoweredBy), IfEnabled(options.WebPageEnabled, info.WebPoweredBy), true),
            new(nameof(IPScannerHostInfo.WebRealm), IfEnabled(options.WebPageEnabled, info.WebRealm), true),
            new(nameof(IPScannerHostInfo.WebRedirectURL), IfEnabled(options.WebPageEnabled, info.WebRedirectURL), true),
            new("WebCertificateSubject", certificate?.Subject, true),
            new("WebCertificateIssuer", certificate?.Issuer, true),
            new("WebCertificateNotAfter", certificate == null
                ? null
                : DateTimeHelper.DateTimeToFullDateTimeString(certificate.NotAfter.ToLocalTime())),
            new("WebCertificateSelfSigned", certificate?.IsSelfSigned)
        ];
    }

    /// <summary>
    ///     Creates a CSV file from the given <see cref="IPScannerHostInfo" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{HostInfo}" /> to export.</param>
    /// <param name="options">Options of the scan the hosts are from.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateCsv(IEnumerable<IPScannerHostInfo> collection, IPScannerResultOptions options,
        string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine(string.Join(",", GetFieldNames()));

        foreach (var info in collection)
        {
            // Skip if critical data is null
            if (info?.PingInfo == null)
                continue;

            stringBuilder.AppendLine(string.Join(",", GetFields(info, options).Select(field =>
            {
                var value = field.Value is List<PortInfo> ports
                    ? string.Join(";", ports.Select(port =>
                        $"{port.Port}/{port.LookupInfo.Protocol}/{port.LookupInfo.Service}/{port.LookupInfo.Description}/{port.State}"))
                    : $"{field.Value}";

                return field.IsQuoted ? CsvHelper.QuoteString(value) : value;
            })));
        }

        File.WriteAllText(filePath, stringBuilder.ToString());
    }

    /// <summary>
    ///     Gets the names of the exported columns (header of the CSV file, also without hosts) - they don't depend on
    ///     the values.
    /// </summary>
    private static IEnumerable<string> GetFieldNames()
    {
        var placeholder = new IPScannerHostInfo(IPAddress.None, false, new PingInfo(), false, [],
            new NetBIOSInfo(IPAddress.None));

        return GetFields(placeholder, new IPScannerResultOptions(false, false, false, false, false, false, false, false,
            false)).Select(x => x.Name);
    }

    /// <summary>
    ///     Creates a XML file from the given <see cref="IPScannerHostInfo" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{HostInfo}" /> to export.</param>
    /// <param name="options">Options of the scan the hosts are from.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateXml(IEnumerable<IPScannerHostInfo> collection, IPScannerResultOptions options,
        string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,
            new XElement(ApplicationName.IPScanner.ToString(),
                new XElement(nameof(IPScannerHostInfo) + "s",
                    from info in collection
                    where info?.PingInfo != null
                    select new XElement(nameof(IPScannerHostInfo),
                        from field in GetFields(info, options)
                        // One element per port, no element for the list
                        from element in field.Value is List<PortInfo> ports
                            ? ports.Select(port => new XElement(nameof(PortInfo),
                                new XElement(nameof(PortInfo.Port), port.Port),
                                new XElement(nameof(PortInfo.LookupInfo.Protocol), port.LookupInfo.Protocol),
                                new XElement(nameof(PortInfo.LookupInfo.Service), port.LookupInfo.Service),
                                new XElement(nameof(PortInfo.LookupInfo.Description), port.LookupInfo.Description),
                                new XElement(nameof(PortInfo.State), port.State)))
                            : [new XElement(field.Name, field.Value)]
                        select element))));

        document.Save(filePath);
    }

    /// <summary>
    ///     Creates a JSON file from the given <see cref="IPScannerHostInfo" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{HostInfo}" /> to export.</param>
    /// <param name="options">Options of the scan the hosts are from.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateJson(IReadOnlyList<IPScannerHostInfo> collection, IPScannerResultOptions options,
        string filePath)
    {
        var jsonData = collection
            .Where(info => info?.PingInfo != null)
            .Select(info => GetFields(info, options).ToDictionary(x => x.Name, x => x.Value switch
            {
                List<PortInfo> ports => ports.Select(port => new
                {
                    port.Port,
                    Protocol = port.LookupInfo.Protocol.ToString(),
                    port.LookupInfo.Service,
                    port.LookupInfo.Description,
                    State = port.State.ToString()
                }).ToArray(),
                // As text, like in the CSV / XML file (not the number of the enum)
                Enum or IPAddress => x.Value.ToString(),
                _ => x.Value
            }))
            .ToArray();

        File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
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
    ///     Gets the HTTP status code of the web page of a host, null if none answered or the web page was not read.
    /// </summary>
    /// <param name="info">Host to get the status code for.</param>
    /// <param name="options">Options of the scan the host is from.</param>
    /// <returns>Status code or null.</returns>
    private static int? GetWebStatusCode(IPScannerHostInfo info, IPScannerResultOptions options)
    {
        return options.WebPageEnabled && info.WebStatusCode > 0 ? info.WebStatusCode : null;
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
        return options.WebPageEnabled ? info.WebCertificate : null;
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
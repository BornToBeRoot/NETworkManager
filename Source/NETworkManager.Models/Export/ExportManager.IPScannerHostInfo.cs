using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    ///     Creates a CSV file from the given <see cref="IPScannerHostInfo" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{HostInfo}" /> to export.</param>
    /// <param name="options">Options of the scan the hosts are from.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateCsv(IEnumerable<IPScannerHostInfo> collection, IPScannerResultOptions options,
        string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine(
            $"{nameof(IPScannerHostInfo.IsReachable)}," +
            $"{nameof(PingInfo.IPAddress)}," +
            $"{nameof(IPScannerHostInfo.Hostname)}," +
            $"PingStatus," +
            $"{nameof(PingInfo.Timestamp)}," +
            $"{nameof(PingInfo.Time)}," +
            $"{nameof(PingInfo.TTL)}," +
            $"{nameof(PingInfo.Bytes)}," +
            $"PortStatus," +
            $"{nameof(IPScannerHostInfo.Ports)}," +
            $"NetBIOSIsReachable," +
            $"NetBIOSIPAddress," +
            $"NetBIOSComputerName," +
            $"NetBIOSUserName," +
            $"NetBIOSGroupName," +
            $"NetBIOSMACAddress," +
            $"NetBIOSVendor," +
            $"{nameof(IPScannerHostInfo.MACAddress)}," +
            $"{nameof(IPScannerHostInfo.Vendor)}," +
            // Added later - last column, so the position of the other columns doesn't change
            $"{nameof(IPScannerHostInfo.DNSHostname)}," +
            $"{nameof(IPScannerHostInfo.DetectedBy)}," +
            $"{nameof(IPScannerHostInfo.OSGuess)}," +
            $"{nameof(IPScannerHostInfo.Role)}," +
            $"{nameof(IPScannerHostInfo.MDNSHostname)}"
        );

        foreach (var info in collection)
        {
            // Skip if critical data is null
            if (info?.PingInfo == null)
                continue;

            // Values of a probe that is still running (export during a scan, or a scan canceled before the probe
            // finished) or disabled are placeholders - exported as empty values instead of a result
            var ping = GetPingInfo(info, options);
            var netBIOSInfo = GetNetBIOSInfo(info, options);

            var stringBuilderPorts = new StringBuilder();

            if (info.Ports != null && options.PortScanEnabled)
            {
                foreach (var port in info.Ports)
                    stringBuilderPorts.Append(
                        $"{port.Port}/{port.LookupInfo.Protocol}/{port.LookupInfo.Service}/{port.LookupInfo.Description}/{port.State};");
            }

            stringBuilder.AppendLine(
                $"{info.IsReachable}," +
                $"{info.PingInfo.IPAddress}," +
                $"{CsvHelper.QuoteString(info.Hostname)}," +
                $"{ping?.Status}," +
                $"{(ping == null ? null : DateTimeHelper.DateTimeToFullDateTimeString(ping.Timestamp))}," +
                $"{(ping == null ? null : Ping.TimeToString(ping.Status, ping.Time, true))}," +
                $"{ping?.TTL}," +
                $"{ping?.Bytes}," +
                $"{GetPortStatus(info, options)}," +
                $"{CsvHelper.QuoteString(stringBuilderPorts.ToString().TrimEnd(';'))}," +
                $"{netBIOSInfo?.IsReachable}," +
                $"{netBIOSInfo?.IPAddress}," +
                $"{CsvHelper.QuoteString(netBIOSInfo?.ComputerName)}," +
                $"{CsvHelper.QuoteString(netBIOSInfo?.UserName)}," +
                $"{CsvHelper.QuoteString(netBIOSInfo?.GroupName)}," +
                $"{netBIOSInfo?.MACAddress}," +
                $"{CsvHelper.QuoteString(netBIOSInfo?.Vendor)}," +
                $"{info.MACAddress}," +
                $"{CsvHelper.QuoteString(info.Vendor)}," +
                $"{CsvHelper.QuoteString(info.DNSHostname)}," +
                $"{CsvHelper.QuoteString(GetDetectedBy(info))}," +
                $"{GetOSGuess(info, options)}," +
                $"{CsvHelper.QuoteString(GetRole(info))}," +
                $"{CsvHelper.QuoteString(options.MDNSEnabled ? info.MDNSHostname : null)}"
            );
        }

        File.WriteAllText(filePath, stringBuilder.ToString());
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
                    // Placeholders of running / disabled probes are exported as empty values (see CreateCsv)
                    let ping = GetPingInfo(info, options)
                    let netBIOSInfo = GetNetBIOSInfo(info, options)
                    select
                        new XElement(nameof(IPScannerHostInfo),
                            new XElement(nameof(IPScannerHostInfo.IsReachable), info.IsReachable),
                            new XElement(nameof(PingInfo.IPAddress), info.PingInfo.IPAddress),
                            new XElement(nameof(IPScannerHostInfo.Hostname), info.Hostname),
                            new XElement("PingStatus", ping?.Status),
                            new XElement(nameof(PingInfo.Timestamp),
                                ping == null ? null : DateTimeHelper.DateTimeToFullDateTimeString(ping.Timestamp)),
                            new XElement(nameof(PingInfo.Time),
                                ping == null ? null : Ping.TimeToString(ping.Status, ping.Time, true)),
                            new XElement(nameof(PingInfo.TTL), ping?.TTL),
                            new XElement(nameof(PingInfo.Bytes), ping?.Bytes),
                            new XElement("PortStatus", GetPortStatus(info, options)),
                            info.Ports != null && options.PortScanEnabled
                                ? from port in info.Ports
                                  select new XElement(nameof(PortInfo),
                                      new XElement(nameof(PortInfo.Port), port.Port),
                                      new XElement(nameof(PortInfo.LookupInfo.Protocol), port.LookupInfo.Protocol),
                                      new XElement(nameof(PortInfo.LookupInfo.Service), port.LookupInfo.Service),
                                      new XElement(nameof(PortInfo.LookupInfo.Description), port.LookupInfo.Description),
                                      new XElement(nameof(PortInfo.State), port.State))
                                : Enumerable.Empty<XElement>(),
                            new XElement("NetBIOSIsReachable", netBIOSInfo?.IsReachable),
                            new XElement("NetBIOSIPAddress", netBIOSInfo?.IPAddress),
                            new XElement("NetBIOSComputerName", netBIOSInfo?.ComputerName),
                            new XElement("NetBIOSUserName", netBIOSInfo?.UserName),
                            new XElement("NetBIOSGroupName", netBIOSInfo?.GroupName),
                            new XElement("NetBIOSMACAddress", netBIOSInfo?.MACAddress),
                            new XElement("NetBIOSVendor", netBIOSInfo?.Vendor),
                            new XElement(nameof(IPScannerHostInfo.MACAddress), info.MACAddress),
                            new XElement(nameof(IPScannerHostInfo.Vendor), info.Vendor),
                            // Added later - last element, so the position of the other elements doesn't change
                            new XElement(nameof(IPScannerHostInfo.DNSHostname), info.DNSHostname),
                            new XElement(nameof(IPScannerHostInfo.DetectedBy), GetDetectedBy(info)),
                            new XElement(nameof(IPScannerHostInfo.OSGuess), GetOSGuess(info, options)),
                            new XElement(nameof(IPScannerHostInfo.Role), GetRole(info)),
                            new XElement(nameof(IPScannerHostInfo.MDNSHostname),
                                options.MDNSEnabled ? info.MDNSHostname : null)
                        )
                )
            )
        );

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
        var validCollection = collection.Where(info => info?.PingInfo != null).ToList();
        var jsonData = new object[validCollection.Count];

        for (var i = 0; i < validCollection.Count; i++)
        {
            var info = validCollection[i];

            // Placeholders of running / disabled probes are exported as empty values (see CreateCsv)
            var ping = GetPingInfo(info, options);
            var netBIOSInfo = GetNetBIOSInfo(info, options);

            object[] jsonDataPorts;

            if (info.Ports != null && info.Ports.Count > 0 && options.PortScanEnabled)
            {
                jsonDataPorts = new object[info.Ports.Count];
                for (var j = 0; j < info.Ports.Count; j++)
                    jsonDataPorts[j] = new
                    {
                        info.Ports[j].Port,
                        Protocol = info.Ports[j].LookupInfo.Protocol.ToString(),
                        info.Ports[j].LookupInfo.Service,
                        info.Ports[j].LookupInfo.Description,
                        State = info.Ports[j].State.ToString()
                    };
            }
            else
            {
                jsonDataPorts = Array.Empty<object>();
            }

            jsonData[i] = new
            {
                info.IsReachable,
                IPAddress = info.PingInfo.IPAddress.ToString(),
                info.Hostname,
                PingStatus = ping?.Status.ToString(),
                Timestamp = ping == null ? null : DateTimeHelper.DateTimeToFullDateTimeString(ping.Timestamp),
                Time = ping == null ? null : Ping.TimeToString(ping.Status, ping.Time, true),
                TTL = ping?.TTL,
                Bytes = ping?.Bytes,
                info.DNSHostname,
                PortStatus = GetPortStatus(info, options)?.ToString(),
                Ports = jsonDataPorts,
                NetBIOSIsReachable = netBIOSInfo?.IsReachable,
                NetBIOSIPAddress = netBIOSInfo?.IPAddress?.ToString(),
                NetBIOSComputerName = netBIOSInfo?.ComputerName,
                NetBIOSUserName = netBIOSInfo?.UserName,
                NetBIOSGroupName = netBIOSInfo?.GroupName,
                NetBIOSMACAddress = netBIOSInfo?.MACAddress,
                NetBIOSVendor = netBIOSInfo?.Vendor,
                info.MACAddress,
                info.Vendor,
                DetectedBy = GetDetectedBy(info),
                OSGuess = GetOSGuess(info, options),
                Role = GetRole(info),
                MDNSHostname = options.MDNSEnabled ? info.MDNSHostname : null
            };
        }

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
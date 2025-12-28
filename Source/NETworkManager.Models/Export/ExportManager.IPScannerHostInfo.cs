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
    public static void Export(string filePath, ExportFileType fileType,
        IReadOnlyList<IPScannerHostInfo> collection)
    {
        switch (fileType)
        {
            case ExportFileType.Csv:
                CreateCsv(collection, filePath);
                break;
            case ExportFileType.Xml:
                CreateXml(collection, filePath);
                break;
            case ExportFileType.Json:
                CreateJson(collection, filePath);
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
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateCsv(IEnumerable<IPScannerHostInfo> collection, string filePath)
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
            $"{nameof(IPScannerHostInfo.ARPMACAddress)}," +
            $"{nameof(IPScannerHostInfo.ARPVendor)}"
        );

        foreach (var info in collection)
        {
            // Skip if critical data is null
            if (info?.PingInfo == null)
                continue;

            var stringBuilderPorts = new StringBuilder();

            if (info.Ports != null)
            {
                foreach (var port in info.Ports)
                    stringBuilderPorts.Append(
                        $"{port.Port}/{port.LookupInfo.Protocol}/{port.LookupInfo.Service}/{port.LookupInfo.Description}/{port.State};");
            }

            stringBuilder.AppendLine(
                $"{info.IsReachable}," +
                $"{info.PingInfo.IPAddress}," +
                $"{info.Hostname}," +
                $"{info.PingInfo.Status}," +
                $"{DateTimeHelper.DateTimeToFullDateTimeString(info.PingInfo.Timestamp)}," +
                $"{Ping.TimeToString(info.PingInfo.Status, info.PingInfo.Time, true)}," +
                $"{info.PingInfo.TTL}," +
                $"{info.PingInfo.Bytes}," +
                $"{(info.IsAnyPortOpen ? PortState.Open : PortState.Closed)}," +
                $"\"{stringBuilderPorts.ToString().TrimEnd(';')}\"," +
                $"{info.NetBIOSInfo?.IsReachable}," +
                $"{info.NetBIOSInfo?.IPAddress}," +
                $"{info.NetBIOSInfo?.ComputerName}," +
                $"{info.NetBIOSInfo?.UserName}," +
                $"{info.NetBIOSInfo?.GroupName}," +
                $"{info.NetBIOSInfo?.MACAddress}," +
                $"{info.NetBIOSInfo?.Vendor}," +
                $"{info.MACAddress}," +
                $"\"{info.Vendor}\"," +
                $"{info.ARPMACAddress}," +
                $"\"{info.ARPVendor}\""
            );
        }

        File.WriteAllText(filePath, stringBuilder.ToString());
    }

    /// <summary>
    ///     Creates a XML file from the given <see cref="IPScannerHostInfo" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{HostInfo}" /> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateXml(IEnumerable<IPScannerHostInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,
            new XElement(ApplicationName.IPScanner.ToString(),
                new XElement(nameof(IPScannerHostInfo) + "s",
                    from info in collection
                    where info?.PingInfo != null
                    select
                        new XElement(nameof(IPScannerHostInfo),
                            new XElement(nameof(IPScannerHostInfo.IsReachable), info.IsReachable),
                            new XElement(nameof(PingInfo.IPAddress), info.PingInfo.IPAddress),
                            new XElement(nameof(IPScannerHostInfo.Hostname), info.Hostname),
                            new XElement("PingStatus", info.PingInfo.Status),
                            new XElement(nameof(PingInfo.Timestamp),
                                DateTimeHelper.DateTimeToFullDateTimeString(info.PingInfo.Timestamp)),
                            new XElement(nameof(PingInfo.Time),
                                Ping.TimeToString(info.PingInfo.Status, info.PingInfo.Time, true)),
                            new XElement(nameof(PingInfo.TTL), info.PingInfo.TTL),
                            new XElement(nameof(PingInfo.Bytes), info.PingInfo.Bytes),
                            new XElement("PortStatus", info.IsAnyPortOpen ? PortState.Open : PortState.Closed),
                            info.Ports != null
                                ? from port in info.Ports
                                  select new XElement(nameof(PortInfo),
                                      new XElement(nameof(PortInfo.Port), port.Port),
                                      new XElement(nameof(PortInfo.LookupInfo.Protocol), port.LookupInfo.Protocol),
                                      new XElement(nameof(PortInfo.LookupInfo.Service), port.LookupInfo.Service),
                                      new XElement(nameof(PortInfo.LookupInfo.Description), port.LookupInfo.Description),
                                      new XElement(nameof(PortInfo.State), port.State))
                                : null,
                            new XElement("NetBIOSIsReachable", info.NetBIOSInfo?.IsReachable),
                            new XElement("NetBIOSIPAddress", info.NetBIOSInfo?.IPAddress),
                            new XElement("NetBIOSComputerName", info.NetBIOSInfo?.ComputerName),
                            new XElement("NetBIOSUserName", info.NetBIOSInfo?.UserName),
                            new XElement("NetBIOSGroupName", info.NetBIOSInfo?.GroupName),
                            new XElement("NetBIOSMACAddress", info.NetBIOSInfo?.MACAddress),
                            new XElement("NetBIOSVendor", info.NetBIOSInfo?.Vendor),
                            new XElement(nameof(IPScannerHostInfo.MACAddress), info.MACAddress),
                            new XElement(nameof(IPScannerHostInfo.Vendor), info.Vendor),
                            new XElement(nameof(IPScannerHostInfo.ARPMACAddress), info.ARPMACAddress),
                            new XElement(nameof(IPScannerHostInfo.ARPVendor), info.ARPVendor)
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
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateJson(IReadOnlyList<IPScannerHostInfo> collection, string filePath)
    {
        var validCollection = collection.Where(info => info?.PingInfo != null).ToList();
        var jsonData = new object[validCollection.Count];

        for (var i = 0; i < validCollection.Count; i++)
        {
            var info = validCollection[i];
            var jsonDataPorts = new object[info.Ports?.Count ?? 0];

            if (info.Ports != null)
            {
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

            jsonData[i] = new
            {
                info.IsReachable,
                IPAddress = info.PingInfo.IPAddress.ToString(),
                info.Hostname,
                PingStatus = info.PingInfo.Status.ToString(),
                Timestamp = DateTimeHelper.DateTimeToFullDateTimeString(info.PingInfo.Timestamp),
                Time = Ping.TimeToString(info.PingInfo.Status, info.PingInfo.Time, true),
                info.PingInfo.TTL,
                info.PingInfo.Bytes,
                info.DNSHostname,
                PortStatus = info.IsAnyPortOpen ? PortState.Open.ToString() : PortState.Closed.ToString(),
                Ports = jsonDataPorts,
                NetBIOSIsReachable = info.NetBIOSInfo?.IsReachable,
                NetBIOSIPAddress = info.NetBIOSInfo?.IPAddress?.ToString(),
                NetBIOSComputerName = info.NetBIOSInfo?.ComputerName,
                NetBIOSUserName = info.NetBIOSInfo?.UserName,
                NetBIOSGroupName = info.NetBIOSInfo?.GroupName,
                NetBIOSMACAddress = info.NetBIOSInfo?.MACAddress,
                NetBIOSVendor = info.NetBIOSInfo?.Vendor,
                info.MACAddress,
                info.Vendor,
                info.ARPMACAddress,
                info.ARPVendor
            };
        }

        File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }
}
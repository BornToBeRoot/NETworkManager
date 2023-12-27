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
            $"{nameof(PingInfo.Bytes)}" +
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
            var stringBuilderPorts = new StringBuilder();

            foreach (var port in info.Ports)
                stringBuilderPorts.Append(
                    $"{port.Port}/{port.LookupInfo.Protocol}/{port.LookupInfo.Service}/{port.LookupInfo.Description}/{port.State};");

            stringBuilder.AppendLine(
                $"{info.IsReachable}," +
                $"{info.PingInfo.IPAddress}," +
                $"{info.Hostname}," +
                $"{info.PingInfo.Status}," +
                $"{DateTimeHelper.DateTimeToFullDateTimeString(info.PingInfo.Timestamp)}," +
                $"{Ping.TimeToString(info.PingInfo.Status, info.PingInfo.Time, true)}," +
                $"{info.PingInfo.TTL}," +
                $"{info.PingInfo.Bytes}" +
                $"{(info.IsAnyPortOpen ? PortState.Open : PortState.Closed)}," +
                $"\"{stringBuilderPorts.ToString().TrimEnd(';')}\"," +
                $"{info.NetBIOSInfo.IsReachable}," +
                $"{info.NetBIOSInfo.IPAddress}," +
                $"{info.NetBIOSInfo.ComputerName}," +
                $"{info.NetBIOSInfo.UserName}," +
                $"{info.NetBIOSInfo.GroupName}," +
                $"{info.NetBIOSInfo.MACAddress}," +
                $"{info.NetBIOSInfo.Vendor}," +
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
                            from port in info.Ports
                            select new XElement(nameof(PortInfo),
                                new XElement(nameof(PortInfo.Port), port.Port),
                                new XElement(nameof(PortInfo.LookupInfo.Protocol), port.LookupInfo.Protocol),
                                new XElement(nameof(PortInfo.LookupInfo.Service), port.LookupInfo.Service),
                                new XElement(nameof(PortInfo.LookupInfo.Description), port.LookupInfo.Description),
                                new XElement(nameof(PortInfo.State), port.State)),
                            new XElement("NetBIOSIsReachable", info.NetBIOSInfo.IsReachable),
                            new XElement("NetBIOSIPAddress", info.NetBIOSInfo.IPAddress),
                            new XElement("NetBIOSComputerName", info.NetBIOSInfo.ComputerName),
                            new XElement("NetBIOSUserName", info.NetBIOSInfo.UserName),
                            new XElement("NetBIOSGroupName", info.NetBIOSInfo.GroupName),
                            new XElement("NetBIOSMACAddress", info.NetBIOSInfo.MACAddress),
                            new XElement("NetBIOSVendor", info.NetBIOSInfo.Vendor),
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
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
        {
            var jsonDataPorts = new object[collection[i].Ports.Count];

            for (var j = 0; j < collection[i].Ports.Count; j++)
                jsonDataPorts[j] = new
                {
                    collection[i].Ports[j].Port,
                    Protocol = collection[i].Ports[j].LookupInfo.Protocol.ToString(),
                    collection[i].Ports[j].LookupInfo.Service,
                    collection[i].Ports[j].LookupInfo.Description,
                    State = collection[i].Ports[j].State.ToString()
                };

            jsonData[i] = new
            {
                collection[i].IsReachable,
                IPAddress = collection[i].PingInfo.IPAddress.ToString(),
                collection[i].Hostname,
                PingStatus = collection[i].PingInfo.Status.ToString(),
                Timestamp = DateTimeHelper.DateTimeToFullDateTimeString(collection[i].PingInfo.Timestamp),
                Time = Ping.TimeToString(collection[i].PingInfo.Status, collection[i].PingInfo.Time, true),
                collection[i].PingInfo.TTL,
                collection[i].PingInfo.Bytes,
                collection[i].DNSHostname,
                PortStatus = collection[i].IsAnyPortOpen ? PortState.Open.ToString() : PortState.Closed.ToString(),
                Ports = jsonDataPorts,
                NetBIOSIsReachable = collection[i].NetBIOSInfo.IsReachable,
                NetBIOSIPAddress = collection[i].NetBIOSInfo.IPAddress?.ToString(),
                NetBIOSComputerName = collection[i].NetBIOSInfo.ComputerName,
                NetBIOSUserName = collection[i].NetBIOSInfo.UserName,
                NetBIOSGroupName = collection[i].NetBIOSInfo.GroupName,
                NetBIOSMACAddress = collection[i].NetBIOSInfo.MACAddress,
                NetBIOSVendor = collection[i].NetBIOSInfo.Vendor,
                collection[i].MACAddress,
                collection[i].Vendor,
                collection[i].ARPMACAddress,
                collection[i].ARPVendor
            };
        }

        File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }
}
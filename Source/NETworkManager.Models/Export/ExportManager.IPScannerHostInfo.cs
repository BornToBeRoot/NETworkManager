using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NETworkManager.Models.Network;
using Newtonsoft.Json;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    /// Method to export objects from type <see cref="IPScannerHostInfo"/> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType"/> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{HostInfo}"/> to export.</param>
    public static void Export(string filePath, ExportFileType fileType,
        IReadOnlyList<IPScannerHostInfo> collection)
    {
        switch (fileType)
        {
            case ExportFileType.CSV:
                CreateCsv(collection, filePath);
                break;
            case ExportFileType.XML:
                CreateXml(collection, filePath);
                break;
            case ExportFileType.JSON:
                CreateJson(collection, filePath);
                break;
            case ExportFileType.TXT:
            default:
                throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
        }
    }

    private static void CreateCsv(IEnumerable<IPScannerHostInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine(
            $"Status,{nameof(PingInfo.IPAddress)},{nameof(IPScannerHostInfo.Hostname)},PortStatus,PingStatus,{nameof(IPScannerHostInfo.MACAddress)},{nameof(IPScannerHostInfo.Vendor)},{nameof(IPScannerHostInfo.Ports)},{nameof(PingInfo.Bytes)},{nameof(PingInfo.Time)},{nameof(PingInfo.TTL)}");

        foreach (var info in collection)
        {
            var stringBuilderPorts = new StringBuilder();

            foreach (var port in info.Ports)
                stringBuilderPorts.Append(
                    $"{port.Port}/{port.LookupInfo.Protocol}/{port.LookupInfo.Service}/{port.LookupInfo.Description}/{port.State};");

            stringBuilder.AppendLine(
                $"{info.IsReachable},{info.PingInfo.IPAddress},{info.Hostname},{(info.IsAnyPortOpen ? PortState.Open : PortState.Closed)},{info.PingInfo.Status},{info.MACAddress},\"{info.Vendor}\",\"{stringBuilderPorts.ToString().TrimEnd(';')}\",{info.PingInfo.Bytes},{Ping.TimeToString(info.PingInfo.Status, info.PingInfo.Time, true)},{info.PingInfo.TTL}");
        }

        System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
    }

    private static void CreateXml(IEnumerable<IPScannerHostInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,
            new XElement(ApplicationName.IPScanner.ToString(),
                new XElement(nameof(IPScannerHostInfo) + "s",
                    from info in collection
                    select
                        new XElement(nameof(IPScannerHostInfo),
                            new XElement("Status", info.IsReachable),
                            new XElement(nameof(PingInfo.IPAddress), info.PingInfo.IPAddress),
                            new XElement(nameof(IPScannerHostInfo.Hostname), info.Hostname),
                            new XElement("PortStatus", info.IsAnyPortOpen ? PortState.Open : PortState.Closed),
                            new XElement("PingStatus", info.PingInfo.Status),
                            new XElement(nameof(IPScannerHostInfo.MACAddress), info.MACAddress),
                            new XElement(nameof(IPScannerHostInfo.Vendor), info.Vendor),
                            from port in info.Ports
                            select new XElement(nameof(PortInfo),
                                new XElement(nameof(PortInfo.Port), port.Port),
                                new XElement(nameof(PortInfo.LookupInfo.Protocol), port.LookupInfo.Protocol),
                                new XElement(nameof(PortInfo.LookupInfo.Service), port.LookupInfo.Service),
                                new XElement(nameof(PortInfo.LookupInfo.Description), port.LookupInfo.Description),
                                new XElement(nameof(PortInfo.State), port.State)),
                            new XElement(nameof(PingInfo.Bytes), info.PingInfo.Bytes),
                            new XElement(nameof(PingInfo.Time),
                                Ping.TimeToString(info.PingInfo.Status, info.PingInfo.Time, true)),
                            new XElement(nameof(PingInfo.TTL), info.PingInfo.TTL)))));

        document.Save(filePath);
    }

    private static void CreateJson(IReadOnlyList<IPScannerHostInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
        {
            var jsonDataPorts = new object[collection[i].Ports.Count];

            for (var j = 0; j < collection[i].Ports.Count; j++)
            {
                jsonDataPorts[j] = new
                {
                    collection[i].Ports[j].Port,
                    Protocol = collection[i].Ports[j].LookupInfo.Protocol.ToString(),
                    collection[i].Ports[j].LookupInfo.Service,
                    collection[i].Ports[j].LookupInfo.Description,
                    State = collection[i].Ports[j].State.ToString()
                };
            }

            jsonData[i] = new
            {
                Status = collection[i].IsReachable.ToString(),
                IPAddress = collection[i].PingInfo.IPAddress.ToString(),
                collection[i].Hostname,
                PortStatus = collection[i].IsAnyPortOpen ? PortState.Open.ToString() : PortState.Closed.ToString(),
                PingStatus = collection[i].PingInfo.Status.ToString(),
                MACAddress = collection[i].MACAddress?.ToString(),
                collection[i].Vendor,
                Ports = jsonDataPorts,
                collection[i].PingInfo.Bytes,
                Time = Ping.TimeToString(collection[i].PingInfo.Status, collection[i].PingInfo.Time, true),
                collection[i].PingInfo.TTL
            };
        }

        System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }
}
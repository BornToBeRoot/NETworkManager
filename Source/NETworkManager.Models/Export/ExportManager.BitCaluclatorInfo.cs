﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NETworkManager.Models.Network;
using Newtonsoft.Json;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    ///     Method to export objects from type <see cref="BitCaluclatorInfo" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{BitCaluclatorInfo}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<BitCaluclatorInfo> collection)
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
    ///     Creates a CSV file from the given <see cref="BitCaluclatorInfo" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{BitCaluclatorInfo}" /> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateCsv(IEnumerable<BitCaluclatorInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        /*
        stringBuilder.AppendLine(
            $"{nameof(NetworkInterfaceInfo.Id)},{nameof(NetworkInterfaceInfo.Name)},{nameof(NetworkInterfaceInfo.Description)},{nameof(NetworkInterfaceInfo.Type)},{nameof(NetworkInterfaceInfo.PhysicalAddress)},{nameof(NetworkInterfaceInfo.Status)},{nameof(NetworkInterfaceInfo.IsOperational)},{nameof(NetworkInterfaceInfo.Speed)},{nameof(NetworkInterfaceInfo.IPv4ProtocolAvailable)},{nameof(NetworkInterfaceInfo.IPv4Address)},{nameof(NetworkInterfaceInfo.IPv4Gateway)},{nameof(NetworkInterfaceInfo.DhcpEnabled)},{nameof(NetworkInterfaceInfo.DhcpServer)},{nameof(NetworkInterfaceInfo.DhcpLeaseObtained)},{nameof(NetworkInterfaceInfo.DhcpLeaseExpires)},{nameof(NetworkInterfaceInfo.IPv6ProtocolAvailable)},{nameof(NetworkInterfaceInfo.IPv6Address)},{nameof(NetworkInterfaceInfo.IPv6AddressLinkLocal)},{nameof(NetworkInterfaceInfo.IPv6Gateway)},{nameof(NetworkInterfaceInfo.DNSAutoconfigurationEnabled)},{nameof(NetworkInterfaceInfo.DNSSuffix)},{nameof(NetworkInterfaceInfo.DNSServer)}");
        
        foreach (var info in collection)
            stringBuilder.AppendLine(
                $"{info.Id},{info.Name},{info.Description},{info.Type},{info.PhysicalAddress},{info.Status},{info.IsOperational},{info.Speed},{info.IPv4ProtocolAvailable},{info.IPv4Address},{info.IPv4Gateway},{info.DhcpEnabled},{info.DhcpServer},{info.DhcpLeaseObtained},{info.DhcpLeaseExpires},{info.IPv6ProtocolAvailable},{info.IPv6Address},{info.IPv6AddressLinkLocal},{info.IPv6Gateway},{info.DNSAutoconfigurationEnabled},{info.DNSSuffix},{info.DNSServer}");
        */
        
        File.WriteAllText(filePath, stringBuilder.ToString());
    }

    /// <summary>
    ///     Creates a XML file from the given <see cref="BitCaluclatorInfo" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{BitCaluclatorInfo}" /> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateXml(IEnumerable<BitCaluclatorInfo> collection, string filePath)
    {
        /*
        var document = new XDocument(DefaultXDeclaration,
            new XElement(ApplicationName.ARPTable.ToString(),
                new XElement(nameof(ARPInfo) + "s",
                    from info in collection
                    select
                        new XElement(nameof(ARPInfo),
                            new XElement(nameof(ARPInfo.IPAddress), info.IPAddress),
                            new XElement(nameof(ARPInfo.MACAddress), info.MACAddress),
                            new XElement(nameof(ARPInfo.IsMulticast), info.IsMulticast)))));

        document.Save(filePath);
        */
    }

    /// <summary>
    ///     Creates a JSON file from the given <see cref="BitCaluclatorInfo" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{BitCaluclatorInfo}" /> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateJson(IReadOnlyList<BitCaluclatorInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        /*
        for (var i = 0; i < collection.Count; i++)
            jsonData[i] = new
            {
                IPAddress = collection[i].IPAddress.ToString(),
                MACAddress = collection[i].MACAddress.ToString(),
                collection[i].IsMulticast
            };
*/
        File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }
}
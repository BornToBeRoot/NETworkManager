using System;
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
    ///     Method to export objects from type <see cref="NetworkInterfaceInfo" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{NetworkInterfaceInfo}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<NetworkInterfaceInfo> collection)
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
    ///     Creates a CSV file from the given <see cref="NetworkInterfaceInfo" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{NetworkInterfaceInfo}" /> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateCsv(IEnumerable<NetworkInterfaceInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine(
            $"{nameof(NetworkInterfaceInfo.Id)},{nameof(NetworkInterfaceInfo.Name)},{nameof(NetworkInterfaceInfo.Description)},{nameof(NetworkInterfaceInfo.Type)},{nameof(NetworkInterfaceInfo.PhysicalAddress)},{nameof(NetworkInterfaceInfo.Status)},{nameof(NetworkInterfaceInfo.IsOperational)},{nameof(NetworkInterfaceInfo.Speed)},{nameof(NetworkInterfaceInfo.IPv4ProtocolAvailable)},{nameof(NetworkInterfaceInfo.IPv4Address)},{nameof(NetworkInterfaceInfo.IPv4Gateway)},{nameof(NetworkInterfaceInfo.DhcpEnabled)},{nameof(NetworkInterfaceInfo.DhcpServer)},{nameof(NetworkInterfaceInfo.DhcpLeaseObtained)},{nameof(NetworkInterfaceInfo.DhcpLeaseExpires)},{nameof(NetworkInterfaceInfo.IPv6ProtocolAvailable)},{nameof(NetworkInterfaceInfo.IPv6Address)},{nameof(NetworkInterfaceInfo.IPv6AddressLinkLocal)},{nameof(NetworkInterfaceInfo.IPv6Gateway)},{nameof(NetworkInterfaceInfo.DNSAutoconfigurationEnabled)},{nameof(NetworkInterfaceInfo.DNSSuffix)},{nameof(NetworkInterfaceInfo.DNSServer)}");

        foreach (var info in collection)
            stringBuilder.AppendLine(
                $"{info.Id},{info.Name},{info.Description},{info.Type},{info.PhysicalAddress},{info.Status},{info.IsOperational},{info.Speed},{info.IPv4ProtocolAvailable},{IPv4Address.ConvertIPAddressWithSubnetmaskListToString(info.IPv4Address, ";")},{IPv4Address.ConvertIPAddressListToString(info.IPv4Gateway, ";")},{info.DhcpEnabled},{IPv4Address.ConvertIPAddressListToString(info.DhcpServer, ";")},{info.DhcpLeaseObtained},{info.DhcpLeaseExpires},{info.IPv6ProtocolAvailable},{IPv4Address.ConvertIPAddressListToString(info.IPv6Address, ";")},{IPv4Address.ConvertIPAddressListToString(info.IPv6AddressLinkLocal, ";")},{IPv4Address.ConvertIPAddressListToString(info.IPv6Gateway, ";")},{info.DNSAutoconfigurationEnabled},{info.DNSSuffix},{IPv4Address.ConvertIPAddressListToString(info.DNSServer, ";")}");

        File.WriteAllText(filePath, stringBuilder.ToString());
    }

    /// <summary>
    ///     Creates a XML file from the given <see cref="NetworkInterfaceInfo" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{NetworkInterfaceInfo}" /> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateXml(IEnumerable<NetworkInterfaceInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,
            new XElement(ApplicationName.NetworkInterface.ToString(),
                new XElement(nameof(NetworkInterfaceInfo) + "s",
                    from info in collection
                    select
                        new XElement(nameof(NetworkInterfaceInfo),
                            new XElement(nameof(NetworkInterfaceInfo.Id), info.Id),
                            new XElement(nameof(NetworkInterfaceInfo.Name), info.Name),
                            new XElement(nameof(NetworkInterfaceInfo.Description), info.Description),
                            new XElement(nameof(NetworkInterfaceInfo.Type), info.Type),
                            new XElement(nameof(NetworkInterfaceInfo.PhysicalAddress), info.PhysicalAddress),
                            new XElement(nameof(NetworkInterfaceInfo.Status), info.Status),
                            new XElement(nameof(NetworkInterfaceInfo.IsOperational), info.IsOperational),
                            new XElement(nameof(NetworkInterfaceInfo.Speed), info.Speed),
                            new XElement(nameof(NetworkInterfaceInfo.IPv4ProtocolAvailable),
                                info.IPv4ProtocolAvailable),
                            new XElement(nameof(NetworkInterfaceInfo.IPv4Address),
                                IPv4Address.ConvertIPAddressWithSubnetmaskListToString(info.IPv4Address, ";")),
                            new XElement(nameof(NetworkInterfaceInfo.IPv4Gateway),
                                IPv4Address.ConvertIPAddressListToString(info.IPv4Gateway, ";")),
                            new XElement(nameof(NetworkInterfaceInfo.DhcpEnabled), info.DhcpEnabled),
                            new XElement(nameof(NetworkInterfaceInfo.DhcpServer),
                                IPv4Address.ConvertIPAddressListToString(info.DhcpServer, ";")),
                            new XElement(nameof(NetworkInterfaceInfo.DhcpLeaseObtained), info.DhcpLeaseObtained),
                            new XElement(nameof(NetworkInterfaceInfo.DhcpLeaseExpires), info.DhcpLeaseExpires),
                            new XElement(nameof(NetworkInterfaceInfo.IPv6ProtocolAvailable),
                                info.IPv6ProtocolAvailable),
                            new XElement(nameof(NetworkInterfaceInfo.IPv6Address),
                                IPv4Address.ConvertIPAddressListToString(info.IPv6Address, ";")),
                            new XElement(nameof(NetworkInterfaceInfo.IPv6AddressLinkLocal),
                                IPv4Address.ConvertIPAddressListToString(info.IPv6AddressLinkLocal, ";")),
                            new XElement(nameof(NetworkInterfaceInfo.IPv6Gateway),
                                IPv4Address.ConvertIPAddressListToString(info.IPv6Gateway, ";")),
                            new XElement(nameof(NetworkInterfaceInfo.DNSAutoconfigurationEnabled),
                                info.DNSAutoconfigurationEnabled),
                            new XElement(nameof(NetworkInterfaceInfo.DNSSuffix), info.DNSSuffix),
                            new XElement(nameof(NetworkInterfaceInfo.DNSServer),
                                IPv4Address.ConvertIPAddressListToString(info.DNSServer, ";"))))));

        document.Save(filePath);
    }

    /// <summary>
    ///     Creates a JSON file from the given <see cref="NetworkInterfaceInfo" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{NetworkInterfaceInfo}" /> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateJson(IReadOnlyList<NetworkInterfaceInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
            jsonData[i] = new
            {
                collection[i].Id,
                collection[i].Name,
                collection[i].Description,
                collection[i].Type,
                collection[i].PhysicalAddress,
                collection[i].Status,
                collection[i].IsOperational,
                collection[i].Speed,
                collection[i].IPv4ProtocolAvailable,
                IPv4Address = IPv4Address.ConvertIPAddressWithSubnetmaskListToString(collection[i].IPv4Address, ";"),
                IPv4Gateway = IPv4Address.ConvertIPAddressListToString(collection[i].IPv4Gateway, ";"),
                collection[i].DhcpEnabled,
                DhcpServer = IPv4Address.ConvertIPAddressListToString(collection[i].DhcpServer, ";"),
                collection[i].DhcpLeaseObtained,
                collection[i].DhcpLeaseExpires,
                collection[i].IPv6ProtocolAvailable,
                IPv6Address = IPv4Address.ConvertIPAddressListToString(collection[i].IPv6Address, ";"),
                IPv6AddressLinkLocal =
                    IPv4Address.ConvertIPAddressListToString(collection[i].IPv6AddressLinkLocal, ";"),
                IPv6Gateway = IPv4Address.ConvertIPAddressListToString(collection[i].IPv6Gateway, ";"),
                collection[i].DNSAutoconfigurationEnabled,
                collection[i].DNSSuffix,
                DNSServer = IPv4Address.ConvertIPAddressListToString(collection[i].DNSServer, ";")
            };

        File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }
}
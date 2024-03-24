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
    ///     Method to export objects from type <see cref="DiscoveryProtocolPackageInfo" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{DiscoveryProtocolPackageInfo}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType,
        IReadOnlyList<DiscoveryProtocolPackageInfo> collection)
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
    ///     Creates a CSV file from the given <see cref="DiscoveryProtocolPackageInfo" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{DiscoveryProtocolPackageInfo}" /> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateCsv(IEnumerable<DiscoveryProtocolPackageInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();


        stringBuilder.AppendLine(
            $"{nameof(DiscoveryProtocolPackageInfo.Device)},{nameof(DiscoveryProtocolPackageInfo.DeviceDescription)},{nameof(DiscoveryProtocolPackageInfo.Port)},{nameof(DiscoveryProtocolPackageInfo.PortDescription)},{nameof(DiscoveryProtocolPackageInfo.Model)},{nameof(DiscoveryProtocolPackageInfo.VLAN)},{nameof(DiscoveryProtocolPackageInfo.IPAddress)},{nameof(DiscoveryProtocolPackageInfo.Protocol)},{nameof(DiscoveryProtocolPackageInfo.TimeToLive)}");

        foreach (var info in collection)
            stringBuilder.AppendLine(
                $"{info.Device},{info.DeviceDescription},{info.Port},{info.PortDescription},{info.Model},{info.VLAN},{info.IPAddress},{info.Protocol},{info.TimeToLive}");

        File.WriteAllText(filePath, stringBuilder.ToString());
    }

    /// <summary>
    ///     Creates a XML file from the given <see cref="DiscoveryProtocolPackageInfo" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{DiscoveryProtocolPackageInfo}" /> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateXml(IEnumerable<DiscoveryProtocolPackageInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,
            new XElement(ApplicationName.DiscoveryProtocol.ToString(),
                new XElement(nameof(DiscoveryProtocolPackageInfo) + "s",
                    from info in collection
                    select
                        new XElement(nameof(DiscoveryProtocolPackageInfo),
                            new XElement(nameof(DiscoveryProtocolPackageInfo.Device), info.Device),
                            new XElement(nameof(DiscoveryProtocolPackageInfo.DeviceDescription),
                                info.DeviceDescription),
                            new XElement(nameof(DiscoveryProtocolPackageInfo.Port), info.Port),
                            new XElement(nameof(DiscoveryProtocolPackageInfo.PortDescription), info.PortDescription),
                            new XElement(nameof(DiscoveryProtocolPackageInfo.Model), info.Model),
                            new XElement(nameof(DiscoveryProtocolPackageInfo.VLAN), info.VLAN),
                            new XElement(nameof(DiscoveryProtocolPackageInfo.IPAddress), info.IPAddress),
                            new XElement(nameof(DiscoveryProtocolPackageInfo.Protocol), info.Protocol),
                            new XElement(nameof(DiscoveryProtocolPackageInfo.TimeToLive), info.TimeToLive)))));

        document.Save(filePath);
    }

    /// <summary>
    ///     Creates a JSON file from the given <see cref="DiscoveryProtocolPackageInfo" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{DiscoveryProtocolPackageInfo}" /> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateJson(IReadOnlyList<DiscoveryProtocolPackageInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
            jsonData[i] = new
            {
                collection[i].Device,
                collection[i].DeviceDescription,
                collection[i].Port,
                collection[i].PortDescription,
                collection[i].Model,
                collection[i].VLAN,
                collection[i].IPAddress,
                collection[i].Protocol,
                collection[i].TimeToLive
            };

        File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }
}
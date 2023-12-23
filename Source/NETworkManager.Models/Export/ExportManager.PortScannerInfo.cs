using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NETworkManager.Models.Lookup;
using NETworkManager.Models.Network;
using Newtonsoft.Json;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    ///     Method to export objects from type <see cref="PortScannerPortInfo" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{PortScannerPortInfo}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType,
        IReadOnlyList<PortScannerPortInfo> collection)
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
    ///     Creates a CSV file from the given <see cref="PortScannerPortInfo" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{PortScannerPortInfo}" /> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateCsv(IEnumerable<PortScannerPortInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine(
            $"{nameof(PortScannerPortInfo.IPAddress)},{nameof(PortScannerPortInfo.Hostname)},{nameof(PortScannerPortInfo.Port)},{nameof(PortLookupInfo.Protocol)},{nameof(PortLookupInfo.Service)},{nameof(PortLookupInfo.Description)},{nameof(PortScannerPortInfo.State)}");

        foreach (var info in collection)
            stringBuilder.AppendLine(
                $"{info.IPAddress},{info.Hostname},{info.Port},{info.LookupInfo.Protocol},{info.LookupInfo.Service},\"{info.LookupInfo.Description}\",{info.State}");

        File.WriteAllText(filePath, stringBuilder.ToString());
    }

    /// <summary>
    ///     Creates a XML file from the given <see cref="PortScannerPortInfo" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{PortScannerPortInfo}" /> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateXml(IEnumerable<PortScannerPortInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,
            new XElement(ApplicationName.PortScanner.ToString(),
                new XElement(nameof(PortScannerPortInfo) + "s",
                    from info in collection
                    select
                        new XElement(nameof(PortScannerPortInfo),
                            new XElement(nameof(PortScannerPortInfo.IPAddress), info.IPAddress),
                            new XElement(nameof(PortScannerPortInfo.Hostname), info.Hostname),
                            new XElement(nameof(PortScannerPortInfo.Port), info.Port),
                            new XElement(nameof(PortLookupInfo.Protocol), info.LookupInfo.Protocol),
                            new XElement(nameof(PortLookupInfo.Service), info.LookupInfo.Service),
                            new XElement(nameof(PortLookupInfo.Description), info.LookupInfo.Description),
                            new XElement(nameof(PortScannerPortInfo.State), info.State)))));

        document.Save(filePath);
    }

    /// <summary>
    ///     Creates a JSON file from the given <see cref="PortScannerPortInfo" /> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{PortScannerPortInfo}" /> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateJson(IReadOnlyList<PortScannerPortInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
            jsonData[i] = new
            {
                IPAddress = collection[i].IPAddress.ToString(),
                collection[i].Hostname,
                collection[i].Port,
                Protocol = collection[i].LookupInfo.Protocol.ToString(),
                collection[i].LookupInfo.Service,
                collection[i].LookupInfo.Description,
                Status = collection[i].State.ToString()
            };

        File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }
}
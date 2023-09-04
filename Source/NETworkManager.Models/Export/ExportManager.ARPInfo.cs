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
    /// Method to export objects from type <see cref="ARPInfo"/> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType"/> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{ARPInfo}"/> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<ARPInfo> collection)
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

    private static void CreateCsv(IEnumerable<ARPInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine(
            $"{nameof(ARPInfo.IPAddress)},{nameof(ARPInfo.MACAddress)},{nameof(ARPInfo.IsMulticast)}");

        foreach (var info in collection)
            stringBuilder.AppendLine($"{info.IPAddress},{info.MACAddress},{info.IsMulticast}");

        System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
    }

    private static void CreateXml(IEnumerable<ARPInfo> collection, string filePath)
    {
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
    }

    private static void CreateJson(IReadOnlyList<ARPInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
        {
            jsonData[i] = new
            {
                IPAddress = collection[i].IPAddress.ToString(),
                MACAddress = collection[i].MACAddress.ToString(),
                collection[i].IsMulticast
            };
        }

        System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }
}
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
    ///     Method to export objects from type <see cref="NeighborInfo" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{NeighborInfo}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<NeighborInfo> collection)
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
    ///     Creates a CSV file from the given <see cref="NeighborInfo" /> collection.
    /// </summary>
    private static void CreateCsv(IEnumerable<NeighborInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine(
            $"{nameof(NeighborInfo.IPAddress)},{nameof(NeighborInfo.MACAddress)},{nameof(NeighborInfo.InterfaceAlias)},{nameof(NeighborInfo.InterfaceIndex)},{nameof(NeighborInfo.State)},{nameof(NeighborInfo.AddressFamily)},{nameof(NeighborInfo.IsMulticast)}");

        foreach (var info in collection)
            stringBuilder.AppendLine(
                $"{info.IPAddress},{info.MACAddress},{CsvHelper.QuoteString(info.InterfaceAlias)},{info.InterfaceIndex},{info.State},{info.AddressFamily},{info.IsMulticast}");

        File.WriteAllText(filePath, stringBuilder.ToString());
    }

    /// <summary>
    ///     Creates an XML file from the given <see cref="NeighborInfo" /> collection.
    /// </summary>
    private static void CreateXml(IEnumerable<NeighborInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,
            new XElement(ApplicationName.NeighborTable.ToString(),
                new XElement(nameof(NeighborInfo) + "s",
                    from info in collection
                    select
                        new XElement(nameof(NeighborInfo),
                            new XElement(nameof(NeighborInfo.IPAddress), info.IPAddress),
                            new XElement(nameof(NeighborInfo.MACAddress), info.MACAddress),
                            new XElement(nameof(NeighborInfo.InterfaceAlias), info.InterfaceAlias),
                            new XElement(nameof(NeighborInfo.InterfaceIndex), info.InterfaceIndex),
                            new XElement(nameof(NeighborInfo.State), info.State),
                            new XElement(nameof(NeighborInfo.AddressFamily), info.AddressFamily),
                            new XElement(nameof(NeighborInfo.IsMulticast), info.IsMulticast)))));

        document.Save(filePath);
    }

    /// <summary>
    ///     Creates a JSON file from the given <see cref="NeighborInfo" /> collection.
    /// </summary>
    private static void CreateJson(IReadOnlyList<NeighborInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
            jsonData[i] = new
            {
                IPAddress = collection[i].IPAddress.ToString(),
                MACAddress = collection[i].MACAddress.ToString(),
                collection[i].InterfaceAlias,
                collection[i].InterfaceIndex,
                State = collection[i].State.ToString(),
                AddressFamily = collection[i].AddressFamily.ToString(),
                collection[i].IsMulticast
            };

        File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }
}

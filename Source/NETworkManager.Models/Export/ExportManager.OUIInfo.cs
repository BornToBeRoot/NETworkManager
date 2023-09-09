using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NETworkManager.Models.Lookup;
using Newtonsoft.Json;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    /// Method to export objects from type <see cref="OUIInfo"/> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType"/> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{OUIInfo}"/> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<OUIInfo> collection)
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
    /// Creates a CSV file from the given <see cref="OUIInfo"/> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{OUIInfo}"/> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateCsv(IEnumerable<OUIInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"{nameof(OUIInfo.MACAddress)},{nameof(OUIInfo.Vendor)}");

        foreach (var info in collection)
            stringBuilder.AppendLine($"{info.MACAddress},\"{info.Vendor}\"");

        System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
    }

    /// <summary>
    /// Creates a XML file from the given <see cref="OUIInfo"/> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{OUIInfo}"/> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateXml(IEnumerable<OUIInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,
            new XElement(ApplicationName.Lookup.ToString(),
                new XElement(nameof(OUIInfo) + "s",
                    from info in collection
                    select
                        new XElement(nameof(OUIInfo),
                            new XElement(nameof(OUIInfo.MACAddress), info.MACAddress),
                            new XElement(nameof(OUIInfo.Vendor), info.Vendor)))));

        document.Save(filePath);
    }

    /// <summary>
    /// Creates a JSON file from the given <see cref="OUIInfo"/> collection.
    /// </summary>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{OUIInfo}"/> to export.</param>
    /// <param name="filePath">Path to the export file.</param>
    private static void CreateJson(IReadOnlyList<OUIInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
        {
            jsonData[i] = new
            {
                collection[i].MACAddress,
                collection[i].Vendor
            };
        }

        System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }
}
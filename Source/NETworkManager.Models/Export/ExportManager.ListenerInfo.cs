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
    /// Method to export objects from type <see cref="ListenerInfo"/> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType"/> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{ListenerInfo}"/> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<ListenerInfo> collection)
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

    private static void CreateCsv(IEnumerable<ListenerInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine(
            $"{nameof(ListenerInfo.Protocol)},{nameof(ListenerInfo.IPAddress)},{nameof(ListenerInfo.Port)}");

        foreach (var info in collection)
            stringBuilder.AppendLine($"{info.Protocol},{info.IPAddress},{info.Port}");

        System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
    }

    private static void CreateXml(IEnumerable<ListenerInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,
            new XElement(ApplicationName.Listeners.ToString(),
                new XElement(nameof(ListenerInfo) + "s",
                    from info in collection
                    select
                        new XElement(nameof(ListenerInfo),
                            new XElement(nameof(ListenerInfo.Protocol), info.Protocol),
                            new XElement(nameof(ListenerInfo.IPAddress), info.IPAddress),
                            new XElement(nameof(ListenerInfo.Port), info.Port)))));

        document.Save(filePath);
    }

    private static void CreateJson(IReadOnlyList<ListenerInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
        {
            jsonData[i] = new
            {
                Protocol = collection[i].Protocol.ToString(),
                IPAddress = collection[i].IPAddress.ToString(),
                collection[i].Port
            };
        }

        System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }
}
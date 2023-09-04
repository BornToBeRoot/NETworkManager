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
    /// Method to export objects from type <see cref="TracerouteHopInfo"/> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType"/> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{TracerouteHopInfo}"/> to export.</param>
    public static void Export(string filePath, ExportFileType fileType,
        IReadOnlyList<TracerouteHopInfo> collection)
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

    private static void CreateCsv(IEnumerable<TracerouteHopInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine(
            $"{nameof(TracerouteHopInfo.Hop)},{nameof(TracerouteHopInfo.Time1)},{nameof(TracerouteHopInfo.Time2)},{nameof(TracerouteHopInfo.Time3)},{nameof(TracerouteHopInfo.IPAddress)},{nameof(TracerouteHopInfo.Hostname)},{nameof(TracerouteHopInfo.Status1)},{nameof(TracerouteHopInfo.Status2)},{nameof(TracerouteHopInfo.Status3)}");

        foreach (var info in collection)
            stringBuilder.AppendLine(
                $"{info.Hop},{Ping.TimeToString(info.Status1, info.Time1, true)},{Ping.TimeToString(info.Status2, info.Time2, true)},{Ping.TimeToString(info.Status3, info.Time3, true)},{info.IPAddress},{info.Hostname},{info.Status1},{info.Status2},{info.Status3}");

        System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
    }

    private static void CreateXml(IEnumerable<TracerouteHopInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,
            new XElement(ApplicationName.Traceroute.ToString(),
                new XElement(nameof(TracerouteHopInfo) + "s",
                    from info in collection
                    select
                        new XElement(nameof(TracerouteHopInfo),
                            new XElement(nameof(TracerouteHopInfo.Hop), info.Hop),
                            new XElement(nameof(TracerouteHopInfo.Time1),
                                Ping.TimeToString(info.Status1, info.Time1, true)),
                            new XElement(nameof(TracerouteHopInfo.Time2),
                                Ping.TimeToString(info.Status2, info.Time2, true)),
                            new XElement(nameof(TracerouteHopInfo.Time3),
                                Ping.TimeToString(info.Status3, info.Time3, true)),
                            new XElement(nameof(TracerouteHopInfo.IPAddress), info.IPAddress),
                            new XElement(nameof(TracerouteHopInfo.Hostname), info.Hostname),
                            new XElement(nameof(TracerouteHopInfo.Status1), info.Status1),
                            new XElement(nameof(TracerouteHopInfo.Status2), info.Status2),
                            new XElement(nameof(TracerouteHopInfo.Status3), info.Status3)))));

        document.Save(filePath);
    }

    private static void CreateJson(IReadOnlyList<TracerouteHopInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
        {
            jsonData[i] = new
            {
                collection[i].Hop,
                Time1 = Ping.TimeToString(collection[i].Status1, collection[i].Time1, true),
                Time2 = Ping.TimeToString(collection[i].Status2, collection[i].Time2, true),
                Time3 = Ping.TimeToString(collection[i].Status3, collection[i].Time3, true),
                IPAddress = collection[i].IPAddress.ToString(),
                collection[i].Hostname,
                Status1 = collection[i].Status1.ToString(),
                Status2 = collection[i].Status2.ToString(),
                Status3 = collection[i].Status3.ToString()
            };
        }

        System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }
}
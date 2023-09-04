using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NETworkManager.Models.Network;
using Newtonsoft.Json;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    /// Method to export objects from type <see cref="PingInfo"/> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType"/> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{PingInfo}"/> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<PingInfo> collection)
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

    private static void CreateCsv(IEnumerable<PingInfo> collection, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine(
            $"{nameof(PingInfo.Timestamp)},{nameof(PingInfo.IPAddress)},{nameof(PingInfo.Hostname)},{nameof(PingInfo.Bytes)},{nameof(PingInfo.Time)},{nameof(PingInfo.TTL)},{nameof(PingInfo.Status)}");

        foreach (var info in collection)
            stringBuilder.AppendLine(
                $"{info.Timestamp},{info.IPAddress},{info.Hostname},{info.Bytes},{Ping.TimeToString(info.Status, info.Time, true)},{info.TTL},{info.Status}");

        System.IO.File.WriteAllText(filePath, stringBuilder.ToString());
    }

    private static void CreateXml(IEnumerable<PingInfo> collection, string filePath)
    {
        var document = new XDocument(DefaultXDeclaration,
            new XElement(ApplicationName.PingMonitor.ToString(),
                new XElement(nameof(PingInfo) + "s",
                    from info in collection
                    select
                        new XElement(nameof(PingInfo),
                            new XElement(nameof(PingInfo.Timestamp), info.Timestamp),
                            new XElement(nameof(PingInfo.IPAddress), info.IPAddress),
                            new XElement(nameof(PingInfo.Hostname), info.Hostname),
                            new XElement(nameof(PingInfo.Bytes), info.Bytes),
                            new XElement(nameof(PingInfo.Time), Ping.TimeToString(info.Status, info.Time, true)),
                            new XElement(nameof(PingInfo.TTL), info.TTL),
                            new XElement(nameof(PingInfo.Status), info.Status)))));

        document.Save(filePath);
    }

    private static void CreateJson(IReadOnlyList<PingInfo> collection, string filePath)
    {
        var jsonData = new object[collection.Count];

        for (var i = 0; i < collection.Count; i++)
        {
            jsonData[i] = new
            {
                collection[i].Timestamp,
                IPAddress = collection[i].IPAddress.ToString(),
                collection[i].Hostname,
                collection[i].Bytes,
                Time = Ping.TimeToString(collection[i].Status, collection[i].Time, true),
                collection[i].TTL,
                Status = collection[i].Status.ToString()
            };
        }

        System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonData, Formatting.Indented));
    }
}